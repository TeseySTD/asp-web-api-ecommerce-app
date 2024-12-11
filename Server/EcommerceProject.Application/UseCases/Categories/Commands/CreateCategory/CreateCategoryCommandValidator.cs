using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using FluentValidation;

namespace EcommerceProject.Application.UseCases.Categories.Commands.CreateCategory;

public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        RuleFor(c => c.Name).MustBeCreatedWith(CategoryName.Create);
        
        RuleFor(c => c.Description).MustBeCreatedWith(CategoryDescription.Create);
    }
}