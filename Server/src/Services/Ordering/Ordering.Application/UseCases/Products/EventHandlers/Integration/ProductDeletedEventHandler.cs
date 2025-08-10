using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Ordering.Application.UseCases.Products.EventHandlers.Integration;

public class ProductDeletedEventHandler : IntegrationEventHandler<ProductDeletedEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public ProductDeletedEventHandler(ILogger<IntegrationEventHandler<ProductDeletedEvent>> logger,
        IApplicationDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    public override async Task Handle(ConsumeContext<ProductDeletedEvent> context)
    {
        var productId = ProductId.Create(context.Message.ProductId).Value;
        if (await _dbContext.Products.AnyAsync(p => p.Id == productId)){ 
            await _dbContext.Orders
                .Where(o => o.OrderItems.Any(i => i.ProductId == productId) 
                    && o.Status != OrderStatus.Cancelled 
                    && o.Status != OrderStatus.Completed)
                .ExecuteUpdateAsync(p =>
                    p.SetProperty(ord => ord.Status, OrderStatus.Cancelled)
                );
            await _dbContext.Products.Where(p => p.Id == productId).ExecuteDeleteAsync();
        }
    }
}