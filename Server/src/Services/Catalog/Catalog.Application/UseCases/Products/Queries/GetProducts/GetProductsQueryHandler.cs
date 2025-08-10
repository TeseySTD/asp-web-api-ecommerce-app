using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Categories.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Core.API;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PaginatedResult<ProductReadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<ProductReadDto>>> Handle(GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;
        var filter = request.FilterRequest;

        var query = _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            var term = filter.Title!.ToLower();
            query = query.Where(p =>
                ((string) p.Title).ToLower().Contains(term));
        }
        if (filter.CategoryId.HasValue)
        {
            var categoryId = CategoryId.Create( filter.CategoryId.Value ).Value;
            query = query.Where(p => p.CategoryId != null && p.CategoryId == categoryId);
        }
        if (filter.MinPrice.HasValue)
        {
            var min = filter.MinPrice.Value;
            query = query.Where(p => ((decimal) p.Price) >= min);
        }
        if (filter.MaxPrice.HasValue)
        {
            var max = filter.MaxPrice.Value;
            query = query.Where(p => ((decimal) p.Price) <= max);
        }

        var productDtos = await query 
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ProjectToType<ProductReadDto>()
            .ToListAsync(cancellationToken);

        if (!productDtos.Any())
            return Error.NotFound;

        return new PaginatedResult<ProductReadDto>(
            pageIndex,
            pageSize,
            productDtos.ToList().AsReadOnly()
        );
    }
}