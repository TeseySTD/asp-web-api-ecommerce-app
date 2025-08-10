using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;

namespace Ordering.Application.UseCases.Orders.EventHandlers.Integration;

public class ApprovedOrderEventHandler : IntegrationEventHandler<ApprovedOrderEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public ApprovedOrderEventHandler(ILogger<IntegrationEventHandler<ApprovedOrderEvent>> logger,
        IApplicationDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }


    public override async Task Handle(ConsumeContext<ApprovedOrderEvent> context)
    {
        var orderId = OrderId.Create(context.Message.OrderId).Value;
        var order = await _dbContext.Orders.FindAsync(orderId);

        var orderItems = await AddOrderItems(context.Message.OrderItemsDtos, orderId);
        
        order!.Approve(orderItems);

        await _dbContext.SaveChangesAsync(default);
    }

    private async Task<List<OrderItem>> AddOrderItems(IEnumerable<OrderItemApprovedDto> orderItemApprovedDtos,
        OrderId orderId)
    {
        var orderItems = new List<OrderItem>();
        foreach (var item in orderItemApprovedDtos)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(p => p.Id == ProductId.Create(item.Id).Value);

            if (product == null)
            {
                product = Product.Create(
                    id: ProductId.Create(item.Id).Value,
                    title: ProductTitle.Create(item.ProductTitle).Value,
                    description: ProductDescription.Create(item.ProductDescription).Value
                );

                await _dbContext.Products.AddAsync(product);
            }

            var orderItem = OrderItem.Create(
                orderId: orderId,
                product: product,
                quantity: OrderItemQuantity.Create(item.Quantity).Value,
                price: OrderItemPrice.Create(item.UnitPrice).Value
            );

            orderItems.Add(orderItem);
        }

        return orderItems;
    }
}