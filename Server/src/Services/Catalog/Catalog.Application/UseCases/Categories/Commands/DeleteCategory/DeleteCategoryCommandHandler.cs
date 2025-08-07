using Catalog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public DeleteCategoryCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await DeleteCategory(request);

        if (result.IsSuccess)
            await _cache.RemoveAsync($"category-{request.Id.Value}");

        return result;
    }

    private async Task<Result> DeleteCategory(DeleteCategoryCommand request)
    {
        if (!await _context.Categories.AnyAsync(c => c.Id == request.Id))
            return Error.NotFound;

        await InvalidateProductCache(request);
        
        await _context.Categories.Where(p => p.Id == request.Id).ExecuteDeleteAsync();
        return Result.Success();
    }

    private async Task InvalidateProductCache(DeleteCategoryCommand request)
    {
        var productsIds = await _context.Products
            .Where(p => p.CategoryId == request.Id)
            .AsNoTracking()
            .Select(p => p.Id.Value)
            .ToListAsync();
    
        foreach(var productId in productsIds)
        {
            await _cache.RemoveAsync($"product-{productId}");
        }
    }
}