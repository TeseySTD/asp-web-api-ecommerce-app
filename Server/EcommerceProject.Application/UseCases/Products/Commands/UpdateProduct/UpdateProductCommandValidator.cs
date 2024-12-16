using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Products.Commands.UpdateProduct;

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