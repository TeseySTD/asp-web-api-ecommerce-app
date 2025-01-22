using FluentValidation;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Core.Validation.FluentValidation;

namespace Ordering.Application.UseCases.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => ProductId.Create(e.ProductId)
        );
        
        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => ProductTitle.Create(e.ProductName)
        );

        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => ProductDescription.Create(e.ProductDescription)
        );

        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => OrderItemQuantity.Create(e.Quantity)
        );

        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => OrderItemPrice.Create(e.Price)
        );
        
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