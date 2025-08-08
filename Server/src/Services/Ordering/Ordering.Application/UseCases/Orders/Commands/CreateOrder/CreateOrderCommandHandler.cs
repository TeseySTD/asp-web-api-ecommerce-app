using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;
using Shared.Messaging.Events.Order;

namespace Ordering.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateOrderCommandHandler(IApplicationDbContext context,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Create(Guid.NewGuid()).Value;

        var result = await Result<Guid>.Try()
            .Check(() => request.Value.OrderItems.GroupBy(o => o.ProductId).Any(g => g.Count() > 1),
                new OrderItemIsNotUniqueError())
            .DropIfFail()
            .CheckAsync(async () =>
            {
                var productIds = request.Value.OrderItems
                    .Select(o => ProductId.Create(o.ProductId).Value)
                    .ToList();
                
                var existingProductIds = await _context.Products
                    .AsNoTracking()
                    .Where(p => productIds.Contains(p.Id))
                    .Select(p => p.Id)
                    .ToListAsync();
                
                var missingProductsIds = productIds.Except(existingProductIds).Select(id => id.Value).ToList();
                
                if(missingProductsIds.Any())
                    return new ProductsNotFound(missingProductsIds);
                return Result<Guid>.Success(default);
            })
            .BuildAsync();
        if (result.IsFailure)
            return result;

        var payment = Payment.Create(
            cvv: request.Value.Payment.cvv,
            cardNumber: request.Value.Payment.cardNumber,
            cardName: request.Value.Payment.cardName,
            expiration: request.Value.Payment.expiration,
            paymentMethod: request.Value.Payment.paymentMethod
        ).Value;

        var destinationAddress = Address.Create(
            addressLine: request.Value.DestinationAddress.addressLine,
            state: request.Value.DestinationAddress.state,
            country: request.Value.DestinationAddress.country,
            zipCode: request.Value.DestinationAddress.zipCode
        ).Value;

        var order = Order.Create(
            id: orderId,
            payment: payment,
            destinationAddress: destinationAddress,
            customerId: CustomerId.Create(request.Value.UserId).Value
        );

        await AddOrder(order, request.Value.OrderItems, cancellationToken);
        return order.Id.Value;
    }

    public async Task AddOrder(Order order, IEnumerable<(Guid ProductId, uint Quantity)> orderItems,
        CancellationToken cancellationToken)
    {
        await _context.Orders.AddAsync(order, cancellationToken);
        var makeOrderEvent = new OrderMadeEvent(
            OrderId: order.Id.Value,
            CustomerId: order.CustomerId.Value,
            Products: orderItems.Select(oi =>
                new ProductWithQuantityDto(
                    oi.ProductId,
                    oi.Quantity
                )
            ).ToList()
        );
        await _publishEndpoint.Publish(makeOrderEvent);
        await _context.SaveChangesAsync(cancellationToken);
    }
    
    public sealed record OrderItemIsNotUniqueError() : Error("Order item is not unique.", $"Each order item must be unique.");

    public sealed record ProductsNotFound : Error
    {
        public ProductsNotFound(IEnumerable<Guid> productIds) : base("Products not found!", $"Products in order not found: {string.Join(", ", productIds)}")
        {}
    } 
}