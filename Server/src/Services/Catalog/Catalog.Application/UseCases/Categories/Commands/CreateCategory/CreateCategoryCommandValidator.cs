using Catalog.Core.Models.Categories.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(c => c.Name).MustBeCreatedWith(CategoryName.Create);
        
        RuleFor(c => c.Description).MustBeCreatedWith(CategoryDescription.Create);
    }
}