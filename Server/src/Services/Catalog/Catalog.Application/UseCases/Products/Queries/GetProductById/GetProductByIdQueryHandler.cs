using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Application.Dto.Product;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;

namespace Catalog.Application.UseCases.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductReadDto>
{
    private readonly IApplicationDbContext _context;

    public GetProductByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ProductReadDto>> Handle(GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.Id)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (product == null)
            return Error.NotFound;

        var category = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == product.CategoryId)
            .FirstOrDefaultAsync(cancellationToken);

        var categoryDto = category == null
            ? null
            : new CategoryDto(
                Id: category.Id.Value,
                Name: category.Name.Value,
                Description: category.Description.Value
            );

        var response =
            new ProductReadDto(
                Id: product.Id.Value,
                Title: product.Title.Value,
                Description: product.Description.Value,
                Price: product.Price.Value,
                Quantity: product.StockQuantity.Value,
                Category: categoryDto
            );

        return response;
    }
}