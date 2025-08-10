using FluentValidation;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.Validation.FluentValidation;

namespace Ordering.Application.UseCases.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId).MustBeCreatedWith(CustomerId.Create);
        
        RuleFor(x => x.OrderId).MustBeCreatedWith(OrderId.Create);
        
        RuleFor(x => x.Value.Payment).MustBeCreatedWith(
            (p) => Payment.Create(
                cardName: p.cardName,
                cardNumber: p.cardNumber,
                paymentMethod: p.paymentMethod,
                expiration: p.expiration,
                cvv: p.cvv
            )
        );

        RuleFor(x => x.Value.DestinationAddress).MustBeCreatedWith(
            (a) => Address.Create(
                addressLine: a.addressLine,
                country: a.country,
                state: a.state,
                zipCode: a.zipCode
            )
        );
    }
}