using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public UpdateCategoryCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var updatedCategoryId = CategoryId.Create(request.Value.Id).Value;
        var updatedCategory = Category.Create(
            id: updatedCategoryId,
            name: CategoryName.Create(request.Value.Name).Value,
            description: CategoryDescription.Create(request.Value.Description).Value
        );

        var result = await Update(updatedCategory, cancellationToken);

        if (result.IsSuccess)
            await _cache.SetStringAsync($"category-{updatedCategoryId.Value}",
                JsonSerializer.Serialize(updatedCategory.Adapt<CategoryDto>()), cancellationToken);

        return result;
    }

    public async Task<Result> Update(Category category, CancellationToken cancellationToken = default)
    {
        if (!await _context.Categories.AnyAsync(p => p.Id == category.Id, cancellationToken))
            return Error.NotFound;

        try
        {
            var categoryToUpdate = await _context.Categories.FindAsync(category.Id, cancellationToken);
            categoryToUpdate!.Update(
                name: category.Name,
                description: category.Description);
            
            await _context.SaveChangesAsync(cancellationToken);
            
            await InvalidateProductCache(categoryToUpdate.Id);

            return Result.Success();
        }
        catch (Exception e)
        {
            return new Error(e.Message, e.StackTrace ?? string.Empty);
        }
    }
    
    private async Task InvalidateProductCache(CategoryId categoryId)
    {
        var productsIds = await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .AsNoTracking()
            .Select(p => p.Id.Value)
            .ToListAsync();
    
        foreach(var productId in productsIds)
        {
            await _cache.RemoveAsync($"product-{productId}");
        }
    }
}