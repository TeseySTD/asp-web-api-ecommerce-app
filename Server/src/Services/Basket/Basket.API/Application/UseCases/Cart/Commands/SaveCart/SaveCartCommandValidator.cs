using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Shared.Core.Validation.Result;

namespace Basket.API.Application.UseCases.Cart.Commands.SaveCart;

public class SaveCartCommandValidator : AbstractValidator<SaveCartCommand>
{
    public SaveCartCommandValidator()
    {
        RuleFor(c => c.Dto.UserId).MustBeCreatedWith(UserId.Create);

        RuleForEach(c => c.Dto.Items).MustBeCreatedWith((i) =>
            Result<ProductCartItem>.Try()
                .Combine(
                    ProductTitle.Create(i.Title),
                    ProductPrice.Create(i.Price),
                    StockQuantity.Create(i.StockQuantity),
                    CategoryId.Create(i.Category.Id),
                    CategoryName.Create(i.Title)
                )
                .Build()
        );
        
    }
}