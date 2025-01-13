using Catalog.Core.Models.Categories.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;

namespace Catalog.Application.UseCases.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(c => c.Value.Id).MustBeCreatedWith(CategoryId.Create);
        
        RuleFor(c => c.Value.Name).MustBeCreatedWith(CategoryName.Create);
        
        RuleFor(c => c.Value.Description).MustBeCreatedWith(CategoryDescription.Create);
    }
}