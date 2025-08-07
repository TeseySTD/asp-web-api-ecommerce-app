using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Categories.Commands.DeleteCategoryImage;

public class DeleteCategoryImageCommandValidator : AbstractValidator<DeleteCategoryImageCommand>
{
    public DeleteCategoryImageCommandValidator()
    {
        RuleFor(c => c.CategoryId)
            .NotNull()
            .WithMessage("Category id cannot be null")
            .MustBeCreatedWith(CategoryId.Create);
        
        RuleFor(c => c.ImageId)
            .NotNull()
            .WithMessage("Image id cannot be null")
            .MustBeCreatedWith(ImageId.Create);
    }
}