using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Application.UseCases.Orders.Commands.CreateOrder;

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
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == ProductId.Create(item.ProductId).Value, 
                    cancellationToken);

            if (product == null)
            {
                product = Product.Create(
                    id: ProductId.Create(item.ProductId).Value,
                    title: ProductTitle.Create(item.ProductName).Value,
                    description: ProductDescription.Create(item.ProductDescription).Value
                );
            
                await _context.Products.AddAsync(product, cancellationToken);
            }

            var orderItem = OrderItem.Create(
                orderId: orderId,
                product: product,
                quantity: OrderItemQuantity.Create(item.Quantity).Value,
                price: OrderItemPrice.Create(item.Price).Value
            );

            orderItems.Add(orderItem);
        }
        var order = Order.Create(
            id: orderId,
            payment: payment,
            destinationAddress: destinationAddress,
            orderItems: orderItems,
            customerId: CustomerId.Create(request.Value.UserId).Value
        );

        return (await Add(order, cancellationToken))
            .Map
            (
                onSuccess: () => Result<Guid>.Success(order.Id.Value),
                onFailure: errors => Result<Guid>.Failure(errors)
            );
    }

    public async Task<Result> Add(Order order, CancellationToken cancellationToken)
    {
        var resultBuilder = Result.Try()
            .Check(await _context.Orders.AnyAsync(o => o.Id == order.Id, cancellationToken),
                new Error(nameof(Order), $"Order with id: {order.Id} already exists"))
            .Check(order.OrderItems.Any(o => o.Product == null),
                new Error("Order items error", "Product in order item cannot be null."))
            .DropIfFail()
            .Check(() => order.OrderItems.GroupBy(o => o.ProductId).Any(g => g.Count() > 1),
                new Error("Order items error", "Each order item must be unique."));

        var result = resultBuilder.Build();
        if (result.IsFailure)
            return result;

        await _context.Orders.AddAsync(order, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}