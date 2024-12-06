using EcommerceProject.Core.Models.Products.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.Products.Commands.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Value.Id)
            .NotNull()
            .NotEmpty()
            .WithMessage("Id is required.");

        RuleFor(x => x.Value.Title)
            .NotNull()
            .NotEmpty()
            .WithMessage("Title is required.");
        
        RuleFor(x => x.Value.Title)
            .MinimumLength(ProductTitle.MinTitleLength)
            .MaximumLength(ProductTitle.MaxTitleLength)
            .WithMessage($"Title must be between {ProductTitle.MinTitleLength} " +
                         $"and {ProductTitle.MaxTitleLength} characters.");

        RuleFor(x => x.Value.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("Description is required.");
            
        RuleFor(x => x.Value.Description)  
            .MinimumLength(ProductDescription.MinDescriptionLength)
            .MaximumLength(ProductDescription.MaxDescriptionLength)
            .WithMessage($"Description must be between {ProductDescription.MinDescriptionLength} " +
                         $"and {ProductDescription.MaxDescriptionLength} characters.");
        
        RuleFor(x => x.Value.Price)
            .LessThan(ProductPrice.MaxPrice)
            .GreaterThan(ProductPrice.MinPrice)
            .WithMessage($"Price must be between {ProductPrice.MinPrice} and {ProductPrice.MaxPrice}.");

        RuleFor(x => x.Value.Quantity)
            .NotNull()
            .WithMessage("Quantity is required.");
    }
}