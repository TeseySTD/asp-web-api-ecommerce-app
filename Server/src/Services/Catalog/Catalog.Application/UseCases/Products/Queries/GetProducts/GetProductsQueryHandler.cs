using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Application.Dto.Product;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;

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

        var products = await _context.Products
            .Include(p => p.Category)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var productDtos = products.Select(p =>
        {
            var categoryDto = p.Category == null
                ? null
                : new CategoryDto(
                    Id: p.Category.Id.Value,
                    Name: p.Category.Name.Value,
                    Description: p.Category.Description.Value
                );

            return new ProductReadDto(
                Id: p.Id.Value,
                Title: p.Title.Value,
                Description: p.Description.Value,
                Price: p.Price.Value,
                Quantity: p.StockQuantity.Value,
                Category: categoryDto
            );
        }).ToList();

        return productDtos.ToList().AsReadOnly();
    }
}