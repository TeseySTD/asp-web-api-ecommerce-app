using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductReadDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public GetProductByIdQueryHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<ProductReadDto>> Handle(GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cachedProduct = await _cache.GetStringAsync($"product-{request.Id.Value}", cancellationToken);
        if (!cachedProduct.IsNullOrEmpty())
            return JsonSerializer.Deserialize<ProductReadDto>(cachedProduct!)!;

        var product = await _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .Where(p => p.Id == request.Id)
            .ProjectToType<ProductReadDto>()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return Error.NotFound;

        await _cache.SetStringAsync($"product-{request.Id.Value}", JsonSerializer.Serialize(product),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

        return product;
    }
}