using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var cachedCategory = await _cache.GetStringAsync($"category-{request.Id.Value}");
        if (!cachedCategory.IsNullOrEmpty())
            return JsonSerializer.Deserialize<CategoryDto>(cachedCategory!)!;

        var result = await GetCategoryById(request, cancellationToken);
        if (result.IsSuccess)
            await _cache.SetStringAsync($"category-{request.Id.Value}", JsonSerializer.Serialize(result.Value),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return result;
    }

    private async Task<Result<CategoryDto>> GetCategoryById(GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .ProjectToType<CategoryDto>()
            .FirstOrDefaultAsync(cancellationToken);

        if (result == null)
            return Error.NotFound;

        return result;
    }
}