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

namespace Catalog.Application.UseCases.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public UpdateProductCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == ProductId.Create(request.Value.Id).Value)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
        if (product == null)
            return new Error("Product not found", $"Product to update with id: {request.Value.Id} not found");

        product.Update(
            title: ProductTitle.Create(request.Value.Title).Value,
            description: ProductDescription.Create(request.Value.Description).Value,
            price: ProductPrice.Create(request.Value.Price).Value,
            quantity: StockQuantity.Create(request.Value.Quantity),
            categoryId: request.Value.CategoryId == null
                ? null
                : CategoryId.Create((Guid)request.Value.CategoryId).Value);

        var result = await Update(product, cancellationToken);

        if (result.IsSuccess)
            await _cache.SetStringAsync($"product-{product.Id.Value}",
                JsonSerializer.Serialize(result.Value),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

        return result;
    }

    private async Task<Result<ProductReadDto>> Update(Product product, CancellationToken cancellationToken = default)
    {
        var result = Result<ProductReadDto>.Try()
            .CheckIf(
                product.CategoryId != null,
                !await _context.Categories.AnyAsync(p => p.Id == product.CategoryId),
                new Error("Category not found", $"Category not found, incorrect id:{product.CategoryId}"))
            .Build();
        if (result.IsFailure)
            return result;

        var productToUpdate = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        productToUpdate!.Update(
            title: product.Title,
            description: product.Description,
            price: product.Price,
            quantity: product.StockQuantity,
            categoryId: product.CategoryId!
        );

        await _context.SaveChangesAsync(cancellationToken);

        var productReadDto =  await _context.Products
            .Include(p => p.Category)
            .Where(p => p.Id == productToUpdate.Id)
            .ProjectToType<ProductReadDto>()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return productReadDto!;
    }
}