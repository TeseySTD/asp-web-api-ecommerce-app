using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.ValueObjects;
using MassTransit;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;
using Shared.Messaging.Events.Basket;
using Shared.Messaging.Events.Order;

namespace Basket.API.Application.UseCases.Cart.Commands.CheckoutBasket;

public class CheckoutBasketCommandHandler : ICommandHandler<CheckoutBasketCommand>
{
    private readonly ICartRepository _cartRepository;
    private readonly IPublishEndpoint _publishEndpoint;

    public CheckoutBasketCommandHandler(ICartRepository cartRepository, IPublishEndpoint publishEndpoint)
    {
        _cartRepository = cartRepository;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<Result> Handle(CheckoutBasketCommand request, CancellationToken cancellationToken)
    {
        var userId = UserId.From(request.Dto.UserId);
        var getCartResult = await _cartRepository.GetCartByUserId(userId, cancellationToken);
        if (getCartResult.IsFailure)
            return getCartResult;

        var cart = getCartResult.Value;

        var addressOfEvent = new BasketCheckoutedEventDestinationAddress(
            AddressLine: request.Dto.DestinationAddress.addressLine,
            Country: request.Dto.DestinationAddress.country,
            State: request.Dto.DestinationAddress.state,
            ZipCode: request.Dto.DestinationAddress.zipCode
        );

        var paymentOfEvent = new BasketCheckoutedEventPayment(
            CardName: request.Dto.Payment.cardName,
            CardNumber: request.Dto.Payment.cardNumber,
            Expiration: request.Dto.Payment.expiration,
            Cvv: request.Dto.Payment.cvv,
            PaymentMethod: request.Dto.Payment.paymentMethod
        );
        
        var checkoutEvent = new BasketCheckoutedEvent(
            UserId: request.Dto.UserId,
            DestinationAddress: addressOfEvent,
            Payment: paymentOfEvent,
            Products: cart.Items.Select(x => new ProductWithQuantityDto(x.Id.Value, x.StockQuantity.Value)).ToList()
        );

        var deleteResult = await _cartRepository.DeleteCart(userId, cancellationToken);
        if (deleteResult.IsFailure)
            return deleteResult;
        
        await _publishEndpoint.Publish(checkoutEvent, cancellationToken);
        
        return Result.Success();
    }
}