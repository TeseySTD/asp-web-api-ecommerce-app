using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderId = OrderId.Create(Guid.NewGuid()).Value;

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

        var orderItems = new List<OrderItem>();
        foreach (var item in request.Value.OrderItems)
        {
            var orderItem = OrderItem.Create(
                orderId: orderId,
                productId: ProductId.Create(item.ProductId).Value,
                quantity: OrderItemQuantity.Create(item.Quantity).Value,
                price: ProductPrice.Create(item.Price).Value
            );

            orderItems.Add(orderItem);
        }

        var order = Order.Create(
            id: orderId,
            payment: payment,
            destinationAddress: destinationAddress,
            orderItems: orderItems,
            userId: UserId.Create(request.Value.UserId).Value
        );

        return (await Add(order, cancellationToken))
            .Map<Result<Guid>>
            (
                onSuccess: () => Result<Guid>.Success(order.Id.Value),
                onFailure: errors => Result<Guid>.Failure(errors)
            );
    }
    
    public async Task<Result> Add(Order order, CancellationToken cancellationToken)
    {
        var resultBuilder = Result.TryFail()
            .CheckError(await _context.Orders.AnyAsync(o => o.Id == order.Id, cancellationToken),
                new Error(nameof(Order), $"Order with id: {order.Id} already exists"))
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == order.UserId, cancellationToken),
                new Error(nameof(User), $"User with id: {order.UserId} not exists"))
            .CheckError(order.OrderItems.GroupBy(o => o.ProductId).Any(g => g.Count() > 1),
                new Error("Order items error", "Each order item must be unique."));

        foreach (var item in order.OrderItems)
            resultBuilder.CheckError(!await _context.Products.AnyAsync(p => p.Id == item.ProductId, cancellationToken),
                new Error(nameof(Product), $"Product with id: {item.ProductId} not exists"));


        var result = resultBuilder.Build();
        if (result.IsFailure)
            return result;

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}