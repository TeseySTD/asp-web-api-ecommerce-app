using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Products.ValueObjects;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;

namespace Catalog.Application.UseCases.Products.EventHandlers.Integration;

public sealed class MakeOrderEventHandler : IntegrationEventHandler<MakeOrderEvent>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public MakeOrderEventHandler(ILogger<IntegrationEventHandler<MakeOrderEvent>> logger,
        IApplicationDbContext dbContext, IPublishEndpoint publishEndpoint) : base(logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public override async Task Handle(ConsumeContext<MakeOrderEvent> context)
    {
        if (await ValidateOrder(context))
        {
            await UpdateProductQuantities(context);

            await _publishEndpoint.Publish(new ApprovedOrderEvent(context.Message.OrderId));
        }
    }

    private async Task<bool> ValidateOrder(ConsumeContext<MakeOrderEvent> context)
    {
        var productIds = context.Message.Products
            .Select(p => ProductId.Create(p.ProductId).Value)
            .ToList();

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.StockQuantity.Value })
            .ToListAsync();

        var existingProductIds = products.Select(p => p.Id).ToList();
        var missingProducts = productIds.Except(existingProductIds).ToList();

        if (missingProducts.Any())
        {
            var missingProductsMessage =
                $"Missing products with IDs: {string.Join(", ", missingProducts.Select(id => id.Value))}";
            await PublishCancelOrder(context.Message.OrderId, missingProductsMessage);
            Logger.LogWarning(missingProductsMessage);
            return false;
        }

        foreach (var orderProduct in context.Message.Products)
        {
            var productId = ProductId.Create(orderProduct.ProductId).Value;
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product == null || product.Value < orderProduct.ProductQuantity)
            {
                var insufficientQuantityMessage = $"Insufficient quantity for product {productId.Value}";
                await PublishCancelOrder(context.Message.OrderId, insufficientQuantityMessage);
                Logger.LogWarning(insufficientQuantityMessage);
                return false;
            }
        }

        return true;
    }

    private async Task UpdateProductQuantities(ConsumeContext<MakeOrderEvent> context)
    {
        foreach (var orderProduct in context.Message.Products)
        {
            var productId = ProductId.Create(orderProduct.ProductId).Value;
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
                product.DecreaseProductQuantity(orderProduct.ProductQuantity);
        }

        await _dbContext.SaveChangesAsync(default);
    }

    private async Task PublishCancelOrder(Guid orderId, string reason)
    {
        await _publishEndpoint.Publish(new CancelOrderEvent(
            OrderId: orderId,
            Reason: reason));
    }
}