using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(c => c.Id.Value).MustBeCreatedWith(CategoryId.Create);
    }
}