using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Categories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Core.API;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Queries.GetProducts;

public sealed class
    GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PaginatedResult<ProductReadDto>>
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

        var productDtos = await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
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