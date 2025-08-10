using Catalog.Application.Dto.Image;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Commands.AddCategoryImages;

public class AddCategoryImagesCommandValidator : AbstractValidator<AddCategoryImagesCommand>
{
    public AddCategoryImagesCommandValidator()
    {
        RuleFor(command => command.CategoryId).MustBeCreatedWith(CategoryId.Create);

        RuleForEach(command => command.Images).MustBeCreatedWith(img => Result<ImageDto>.Try()
            .Combine(
                FileName.Create(img.FileName),
                ImageData.Create(img.Data))
            .Build());
    }
}