using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Shared.Core.Validation.Result;

namespace Basket.API.Application.UseCases.Cart.Commands.StoreProduct;

public class StoreProductCommandValidator : AbstractValidator<StoreProductCommand>
{
    public StoreProductCommandValidator()
    {
        RuleFor(x => x.UserId).MustBeCreatedWith(UserId.Create);
        
        RuleFor(x => x.Product).MustBeCreatedWith(i =>
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