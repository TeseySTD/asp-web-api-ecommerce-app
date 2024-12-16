using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Products.ValueObjects;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    private readonly IOrdersRepository _ordersRepository;

    public CreateOrderCommandHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
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

        return (await _ordersRepository.Add(order, cancellationToken))
            .Map<Result<Guid>>
            (
                onSuccess: () => Result<Guid>.Success(order.Id.Value),
                onFailure: errors => Result<Guid>.Failure(errors)
            );
    }
}