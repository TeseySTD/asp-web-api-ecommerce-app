using Basket.API.Models.Cart.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Address = Basket.API.Models.Cart.ValueObjects.Address;
using Payment = Basket.API.Models.Cart.ValueObjects.Payment;

namespace Basket.API.Application.UseCases.Cart.Commands.CheckoutBasket;

public class CheckoutBasketCommandValidator : AbstractValidator<CheckoutBasketCommand>
{
    public CheckoutBasketCommandValidator()
    {
        RuleFor(c => c.Dto).NotNull().WithMessage("Request cannot be null");

        RuleFor(c => c.Dto.UserId).MustBeCreatedWith(UserId.Create);

        RuleFor(c => c.Dto.DestinationAddress)
            .MustBeCreatedWith(a => Address.Create(a.addressLine, a.country, a.state, a.zipCode));

        RuleFor(c => c.Dto.Payment).MustBeCreatedWith(p =>
            Payment.Create(p.cardName, p.cardNumber, p.expiration, p.cvv, p.paymentMethod));
    }
}