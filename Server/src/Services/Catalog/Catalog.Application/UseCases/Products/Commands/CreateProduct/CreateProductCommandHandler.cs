using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public CreateProductCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        Product product = Product.Create(
            id: ProductId.Create(request.Value.Id).Value,
            title: ProductTitle.Create(request.Value.Title).Value,
            description: ProductDescription.Create(request.Value.Description).Value,
            price: ProductPrice.Create(request.Value.Price).Value,
            categoryId: CategoryId.Create(request.Value.CategoryId).Value
        );

        product.StockQuantity = StockQuantity.Create(request.Value.Quantity);

        var result = await Add(product);

        if (result.IsSuccess)
            await _cache.SetStringAsync($"product-{product.Id.Value}",
                JsonSerializer.Serialize(result.Value),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

        return result;
    }

    private async Task<Result<ProductReadDto>> Add(Product product, CancellationToken cancellationToken = default)
    {
        var result = Result<ProductReadDto>.Try()
            .Check(await _context.Products.AnyAsync(p => p.Id == product.Id),
                new Error("Product already exists", $"Product already exists, incorrect id:{product.Id}"))
            .Check(!await _context.Categories.AnyAsync(p => p.Id == product.CategoryId),
                new Error("Category not found", $"Category not found, incorrect id:{product.CategoryId}"))
            .Build();

        if (result.IsSuccess)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync(cancellationToken);

            var productReadDto = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Id == product.Id)
                .ProjectToType<ProductReadDto>()
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            return productReadDto!;
        }

        return result;
    }
}