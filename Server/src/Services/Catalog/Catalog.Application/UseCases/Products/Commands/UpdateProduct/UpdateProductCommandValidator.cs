using Catalog.Core.Models.Products.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Products.Commands.UpdateProduct;

public class UpdateProductCommandValidator: AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Value.Id)
            .NotNull()
            .MustBeCreatedWith(ProductId.Create);
        
        RuleFor(x => x.Value.Title)
            .NotNull()
            .MustBeCreatedWith(ProductTitle.Create);
        
        RuleFor(x => x.Value.Description)
            .NotNull()
            .MustBeCreatedWith(ProductDescription.Create);
        
        RuleFor(x => x.Value.Price)
            .NotNull()
            .MustBeCreatedWith(ProductPrice.Create);

        RuleFor(x => x.Value.Quantity)
            .NotNull()
            .WithMessage("Quantity is required");
        
        RuleFor(x => x.Value.CategoryId).NotEqual(Guid.Empty);
    }
}