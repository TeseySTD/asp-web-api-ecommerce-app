using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Value.Id)
            .MustBeCreatedWith(ProductId.Create);
        
        RuleFor(x => x.Value.Title)
            .MustBeCreatedWith(ProductTitle.Create);
        
        RuleFor(x => x.Value.Description)
            .MustBeCreatedWith(ProductDescription.Create);
        
        RuleFor(x => x.Value.Price)
            .MustBeCreatedWith(ProductPrice.Create);

        RuleFor(x => x.Value.Quantity)
            .NotNull()
            .MustBeCreatedWith(StockQuantity.Create);
        
        RuleFor(x => x.Value.CategoryId)
            .MustBeCreatedWith(CategoryId.Create);
    }
}