using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Products.ValueObjects;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;
using Shared.Messaging.Messages;
using Shared.Messaging.Messages.Order;

namespace Catalog.Application.UseCases.Products.EventHandlers.Integration;

public sealed class ReserveProductsMessageHandler : IntegrationMessageHandler<ReserveProductsMessage>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;

    public ReserveProductsMessageHandler(ILogger<IntegrationMessageHandler<ReserveProductsMessage>> logger,
        IApplicationDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        IDistributedCache cache) : base(logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public override async Task Handle(ConsumeContext<ReserveProductsMessage> context)
    {
        if (await ValidateOrder(context))
        {
            await UpdateProductQuantities(context);

            var orderItems = context.Message.Products;
            var orderItemsIds = orderItems.Select(o => ProductId.Create(o.ProductId).Value);

            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(p => orderItemsIds.Contains(p.Id))
                .Select(p => new
                    { p.Id, Title = p.Title.Value, Description = p.Description.Value, Price = p.Price.Value })
                .ToListAsync();

            var orderItemsDtos = products.Select(p =>
            {
                var quantity = orderItems.First(o => o.ProductId == p.Id.Value).ProductQuantity;
                return new OrderItemApprovedDto(
                    Id: p.Id.Value,
                    ProductTitle: p.Title,
                    ProductDescription: p.Description,
                    Quantity: quantity,
                    UnitPrice: p.Price
                );
            }).ToList();

            await _publishEndpoint.Publish(new ReservedProductsEvent(
                context.Message.OrderId,
                orderItemsDtos
            ));
        }
    }

    private async Task<bool> ValidateOrder(ConsumeContext<ReserveProductsMessage> context)
    {
        var productIds = context.Message.Products
            .Select(p => ProductId.Create(p.ProductId).Value)
            .ToList();

        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, Quantity = p.StockQuantity.Value})
            .ToListAsync();

        var existingProductIds = products.Select(p => p.Id).ToList();
        var missingProducts = productIds.Except(existingProductIds).ToList();

        if (missingProducts.Any())
        {
            var missingProductsMessage =
                $"Missing products with IDs: {string.Join(", ", missingProducts.Select(id => id.Value))}";
            await PublishFailedReservation(context.Message.OrderId, missingProductsMessage);
            Logger.LogWarning(missingProductsMessage);
            return false;
        }

        foreach (var orderProduct in context.Message.Products)
        {
            var productId = ProductId.Create(orderProduct.ProductId).Value;
            var product = products.FirstOrDefault(p => p.Id == productId);

            if (product!.Quantity < orderProduct.ProductQuantity)
            {
                var insufficientQuantityMessage = $"Insufficient quantity for product {productId.Value}";
                await PublishFailedReservation(context.Message.OrderId, insufficientQuantityMessage);
                Logger.LogWarning(insufficientQuantityMessage);
                return false;
            }
        }

        return true;
    }

    private async Task UpdateProductQuantities(ConsumeContext<ReserveProductsMessage> context)
    {
        foreach (var orderProduct in context.Message.Products)
        {
            var productId = ProductId.Create(orderProduct.ProductId).Value;
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product != null)
            {
                product.DecreaseQuantity(orderProduct.ProductQuantity);
                await _cache.RemoveAsync($"product-{product.Id.Value}");
            }
        }

        await _dbContext.SaveChangesAsync(default);
    }

    private async Task PublishFailedReservation(Guid orderId, string reason)
    {
        await _publishEndpoint.Publish(new ReservationProductsFailedEvent(
            OrderId: orderId,
            Reason: reason));
    }
}