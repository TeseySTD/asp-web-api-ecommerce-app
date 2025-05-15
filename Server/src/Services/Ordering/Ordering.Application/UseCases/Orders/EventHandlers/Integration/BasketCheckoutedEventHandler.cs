using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Commands.CreateOrder;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Basket;

namespace Ordering.Application.UseCases.Orders.EventHandlers.Integration;

public class BasketCheckoutedEventHandler : IntegrationEventHandler<BasketCheckoutedEvent>
{
    private readonly ISender _sender;

    public BasketCheckoutedEventHandler(
        ILogger<IntegrationEventHandler<BasketCheckoutedEvent>> logger,
        ISender sender
    ) : base(logger)
    {
        _sender = sender;
    }

    public override async Task Handle(ConsumeContext<BasketCheckoutedEvent> context)
    {
        var address = context.Message.DestinationAddress;
        var payment = context.Message.Payment;

        var orderWriteDto = new OrderWriteDto(
            UserId: context.Message.UserId,
            DestinationAddress: (
                addressLine: address.AddressLine,
                country: address.Country,
                state: address.State,
                zipCode: address.ZipCode
            ),
            Payment: (
                cardName: payment.CardName,
                cardNumber: payment.CardNumber,
                expiration: payment.Expiration,
                cvv: payment.Cvv,
                paymentMethod: payment.PaymentMethod
            ),
            OrderItems: context.Message.Products
                .Select(p => (p.ProductId, p.ProductQuantity)).ToList()
        );
        var cmd = new CreateOrderCommand(orderWriteDto);

        await _sender.Send(cmd);
    }
}