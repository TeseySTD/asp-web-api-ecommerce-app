using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommandValidator : AbstractValidator<DeleteProductImageCommand>
{
    public DeleteProductImageCommandValidator()
    {
        RuleFor(c => c.ProductId)
            .NotNull()
            .WithMessage("Category id cannot be null")
            .MustBeCreatedWith(ProductId.Create);
        
        RuleFor(c => c.ImageId)
            .NotNull()
            .WithMessage("Image id cannot be null")
            .MustBeCreatedWith(ImageId.Create);
    }
}