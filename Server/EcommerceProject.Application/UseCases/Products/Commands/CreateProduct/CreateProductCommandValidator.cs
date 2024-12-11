using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Products.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Value.Id).MustBeCreatedWith(ProductId.Create);
        
        RuleFor(x => x.Value.Title).MustBeCreatedWith(ProductTitle.Create);
        
        RuleFor(x => x.Value.Description).MustBeCreatedWith(ProductDescription.Create);
        
        RuleFor(x => x.Value.Price).MustBeCreatedWith(ProductPrice.Create);

        RuleFor(x => x.Value.Quantity)
            .NotEmpty()
            .WithMessage("Quantity is required.");
    }
}