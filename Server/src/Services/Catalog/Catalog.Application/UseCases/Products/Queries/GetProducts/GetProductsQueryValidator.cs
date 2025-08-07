using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Categories.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Queries.GetProducts;

public class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(cmd => cmd.FilterRequest).MustBeCreatedWith(f => Result<ProductFilterRequest>.Try()
            .CheckIf(
                checkCondition: f.CategoryId is not null,
                (Result<ProductFilterRequest>)CategoryId.Create(f.CategoryId!.Value).Map(
                    onSuccess: () => Result.Success(), onFailure: e => Result.Failure(e))
            )
            .CheckIf(
                checkCondition: f.MinPrice != null,
                f.MinPrice! >= 0,
                new MinPriceLessOrEqualToZero())
            .CheckIf(
                checkCondition: f.MaxPrice != null,
                f.MaxPrice! >= 0,
                new MaxPriceLessOrEqualToZero())
            .DropIfFail()
            .CheckIf(
                checkCondition: f.MinPrice != null && f.MaxPrice != null,
                f.MinPrice > f.MaxPrice,
                new MinPriceLessOrEqualToZero())
            .Build()
        );
    }

    public sealed record MinPriceLessOrEqualToZero() :
        Error("Min Price filter property error", "Min price must be greater than zero.");

    public sealed record MaxPriceLessOrEqualToZero() :
        Error("Max Price filter property error", "Max price must be greater than zero.");
    
    public sealed record MinPriceLargerThenMaxError() :
        Error("Max and Min price properties error", "Min price must be less or equal to the max value.");
}