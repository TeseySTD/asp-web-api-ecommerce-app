using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Commands.DeleteCategoryImage;

public class DeleteCategoryImageCommandHandler : ICommandHandler<DeleteCategoryImageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public DeleteCategoryImageCommandHandler(IDistributedCache cache, IApplicationDbContext context)
    {
        _cache = cache;
        _context = context;
    }

    public async Task<Result> Handle(DeleteCategoryImageCommand request, CancellationToken cancellationToken)
    {
        var (categoryId, imageId) = (CategoryId.Create(request.CategoryId).Value, ImageId.Create(request.ImageId).Value);
        
        var category = await _context.Categories
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == categoryId, cancellationToken);
        var image = await _context.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken);
        
        if (category is null)
            return new Error("Category not found", $"Category not found, incorrect id:{request.CategoryId}");
        if (image is null || !category.Images.Any(i => i.Id == imageId))
            return new Error("Image not found",
                $"Image not found, incorrect id:{request.ImageId} or not belong to category with id:{request.CategoryId}");
        
        category.RemoveImage(imageId);
        await _context.SaveChangesAsync(cancellationToken);
        
        await _cache.SetStringAsync($"category-{category.Id.Value}",
            JsonSerializer.Serialize(category.Adapt<CategoryReadDto>()),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        
        return Result.Success();
    }
}