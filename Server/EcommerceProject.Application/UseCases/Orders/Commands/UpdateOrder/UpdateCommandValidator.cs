using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Products.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Orders.Commands.UpdateOrder;

public class UpdateCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateCommandValidator()
    {
        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => ProductId.Create(e.ProductId)
        );

        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => OrderItemQuantity.Create(e.Quantity)
        );

        RuleForEach(x => x.Value.OrderItems).MustBeCreatedWith(
            (e) => ProductPrice.Create(e.Price)
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