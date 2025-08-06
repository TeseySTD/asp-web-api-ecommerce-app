using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

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
        var result = await ValidateCommand(request, cancellationToken);

        if (result.IsFailure)
            return result;

        var orderToUpdate = result.Value;

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

        orderToUpdate.Update(payment, destiantionAddress);

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private async Task<Result<Order>> ValidateCommand(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Create(request.OrderId).Value;
        var customerId = CustomerId.Create(request.CustomerId).Value;
        Order? orderToUpdate = null;

        var result = await Result<Order>.Try()
            .Check(!await _context.Orders.AnyAsync(o => o.Id == orderId, cancellationToken),
                new OrderNotFoundError(orderId.Value))
            .DropIfFail()
            .CheckAsync(
                async () => !await _context.Orders
                    .AnyAsync(o => o.Id == orderId && o.CustomerId == customerId, cancellationToken),
                new CustomerMismatchError(request.CustomerId))
            .DropIfFail()
            .CheckAsync(async () =>
            {
                orderToUpdate = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

                if (orderToUpdate!.Status == OrderStatus.Cancelled || orderToUpdate!.Status == OrderStatus.Completed)
                    return true;
                return false;
            }, new IncorrectOrderStateError())
            .BuildAsync();

        return result.Map(
            onSuccess: () => orderToUpdate!,
            onFailure: errors => Result<Order>.Failure(errors)
        );
    }

    public sealed record OrderNotFoundError(Guid OrderId)
        : Error("Order for update not found", $"There is no order with this id: {OrderId}");

    public sealed record CustomerMismatchError(Guid CustomerId) : Error("You can`t update this order!",
        $"Your id {CustomerId} doesn’t match with customer’s id in order.");

    public sealed record IncorrectOrderStateError() : Error("Incorrect order state",
        $"The order state cannot be {OrderStatus.Cancelled} or {OrderStatus.Completed}");
}