using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation;

namespace Ordering.Application.UseCases.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : ICommandHandler<UpdateOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var resultBuilder = await Result.TryFail()
            .CheckErrorAsync( async () => !await _context.Orders.AnyAsync(o => o.Id == request.OrderId, cancellationToken),
                new Error("Order for update not found", $"There is no order with this id {request.OrderId} "))
            .DropIfFailed()
            .CheckError(() => request.Value.OrderItems.Any(o => o.ProductId == null),
                new Error("Order items error", "Product in order item cannot be null."))
            .DropIfFailed()
            .CheckError(() => request.Value.OrderItems.GroupBy(o => o.ProductId).Any(g => g.Count() > 1),
                new Error("Order items error", "Each order item must be unique."));

        var result = resultBuilder.Build();
        
        if(result.IsFailure)
            return result;
        
        var orderToUpdate = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        var payment = Payment.Create(
            cvv: request.Value.Payment.cvv,
            cardNumber: request.Value.Payment.cardNumber,
            cardName: request.Value.Payment.cardName,
            expiration: request.Value.Payment.expiration,
            paymentMethod: request.Value.Payment.paymentMethod
        ).Value;

        var destiantionAddress = Address.Create(
            addressLine: request.Value.DestinationAddress.addressLine,
            country: request.Value.DestinationAddress.country,
            state: request.Value.DestinationAddress.state,
            zipCode: request.Value.DestinationAddress.zipCode
        ).Value;

        var orderItems = new List<OrderItem>();
        foreach (var item in request.Value.OrderItems)
        {
            var orderItem = OrderItem.Create(
                orderId: orderToUpdate!.Id,
                productId: ProductId.Create(item.ProductId).Value,
                quantity: OrderItemQuantity.Create(item.Quantity).Value,
                price: OrderItemPrice.Create(item.Price).Value
            );

            orderItems.Add(orderItem);
        }

        orderToUpdate!.Update(orderItems, payment, destiantionAddress);

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }
}