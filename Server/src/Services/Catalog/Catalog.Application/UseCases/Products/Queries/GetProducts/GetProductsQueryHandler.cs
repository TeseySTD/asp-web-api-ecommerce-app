using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Categories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IReadOnlyList<ProductReadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetProductsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyList<ProductReadDto>>> Handle(GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_context.Products.Any())
            return Result<IReadOnlyList<ProductReadDto>>.Failure(Error.NotFound);

        var productDtos = await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ProjectToType<ProductReadDto>()
            .ToListAsync(cancellationToken);

        return productDtos.ToList().AsReadOnly();
    }
}