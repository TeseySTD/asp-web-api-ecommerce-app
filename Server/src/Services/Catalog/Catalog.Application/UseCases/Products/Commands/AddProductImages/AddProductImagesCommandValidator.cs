using Catalog.Application.Dto.Image;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using FluentValidation;
using Shared.Core.Validation.FluentValidation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Commands.AddProductImages;

public class AddProductImagesCommandValidator : AbstractValidator<AddProductImagesCommand>
{
    public AddProductImagesCommandValidator()
    {
        RuleFor(command => command.ProductId).MustBeCreatedWith(ProductId.Create);

        RuleForEach(command => command.Images).MustBeCreatedWith(img => Result<ImageDto>.Try()
            .Combine(
                FileName.Create(img.FileName),
                ImageData.Create(img.Data))
            .Build());
    }
}