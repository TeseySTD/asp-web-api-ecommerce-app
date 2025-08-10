using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Commands.AddCategoryImages;

public class AddCategoryImagesCommandHandler : ICommandHandler<AddCategoryImagesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;


    public AddCategoryImagesCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(AddCategoryImagesCommand request, CancellationToken cancellationToken)
    {
       var result = await ValidateDataAsync(request, cancellationToken); 

        if (result.IsFailure)
            return result;

        var category = result.Value;

        foreach (var imageDto in request.Images)
        {
            var fileName = FileName.Create(imageDto.FileName).Value;
            var data = ImageData.Create(imageDto.Data).Value;
            var contentType = Enum.Parse<ImageContentType>(imageDto.ContentType, ignoreCase: true);

            var image = Image.Create(fileName, data, contentType);
            await _context.Images.AddAsync(image);
            category.AddImage(image);
        }

        await _context.SaveChangesAsync(default);

        await _cache.SetStringAsync($"category-{category.Id.Value}",
            JsonSerializer.Serialize(category.Adapt<CategoryReadDto>()),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return Result.Success();
    }

    private async Task<Result<Category>> ValidateDataAsync(AddCategoryImagesCommand request, CancellationToken cancellationToken)
    {
        var categoryId = CategoryId.Create(request.CategoryId).Value;
        Category? category = null;
        var result = await Result.Try()
            .Check(!await _context.Categories.AnyAsync(p => p.Id == categoryId), new CategoryNotFoundError(categoryId.Value))
            .DropIfFail()
            .CheckAsync(async () =>
                {
                    category = await _context.Categories
                            .Include(p => p.Images)
                        .FirstOrDefaultAsync(p => p.Id == categoryId, cancellationToken);

                    return category!.Images.Count + request.Images.Count() > Category.MaxImagesCount;
                },
                new ImagesOutOfRangeError(categoryId.Value))
            .BuildAsync();

        return result.Map(
            onSuccess: () => category!,
            onFailure: e => Result<Category>.Failure(e));
    }

    public sealed record CategoryNotFoundError(Guid CategoryId) : Error("Сategory not found!",
        $"Сategory with id: {CategoryId} was not found!");

    public sealed record ImagesOutOfRangeError(Guid CategoryId) : Error("Max images in category.",
        $"Categoru with id: {CategoryId} has maximum ({Category.MaxImagesCount}) images.");
}