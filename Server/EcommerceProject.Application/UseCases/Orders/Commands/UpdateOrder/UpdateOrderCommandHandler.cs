using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : ICommandHandler<UpdateOrderCommand>
{
    private readonly IOrdersRepository _ordersRepository;

    public UpdateOrderCommandHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<Result> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _ordersRepository.FindById(request.OrderId, cancellationToken);
        if (order == null)
            return new Error("Order for update not found", $"There is no order with this id {request.OrderId} ");

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
                orderId: order.Id,
                productId: ProductId.Create(item.ProductId).Value,
                quantity: OrderItemQuantity.Create(item.Quantity).Value,
                price: ProductPrice.Create(item.Price).Value
            );

            orderItems.Add(orderItem);
        }
        
        order.Update(orderItems, payment, destiantionAddress);
        
        return  await _ordersRepository.Update(order, cancellationToken);
    }
}