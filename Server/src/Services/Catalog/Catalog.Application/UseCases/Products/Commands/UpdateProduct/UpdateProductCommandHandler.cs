using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
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
        var result = await Result.Try()
            .Check(!await _context.Products.AnyAsync(p => p.Id == ProductId.Create(request.Value.Id).Value),
                new ProductNotFoundError(request.Value.Id))
            .CheckIfAsync(
                request.Value.CategoryId != null,
                async () => !await _context.Categories.AnyAsync(p =>
                    p.Id == CategoryId.Create(request.Value.CategoryId ?? Guid.Empty).Value),
                new CategoryNotFoundError(request.Value.CategoryId ?? Guid.Empty))
            .BuildAsync();

        if (result.IsSuccess)
        {
            var product = await _context.Products
                .Where(p => p.Id == ProductId.Create(request.Value.Id).Value)
                .Include(p => p.Category)
                    .ThenInclude(c => c.Images)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(cancellationToken);

            var category = request.Value.CategoryId.HasValue
                ? await _context.Categories
                    .Include(c => c.Images)
                    .FirstOrDefaultAsync(p => p.Id == CategoryId.Create((Guid)request.Value.CategoryId).Value,
                        cancellationToken)
                : null;

            product!.Update(
                title: ProductTitle.Create(request.Value.Title).Value,
                description: ProductDescription.Create(request.Value.Description).Value,
                price: ProductPrice.Create(request.Value.Price).Value,
                quantity: StockQuantity.Create(request.Value.Quantity).Value,
                sellerId: SellerId.Create(request.Value.SellerId).Value,
                category: category
            );

            await _context.SaveChangesAsync(cancellationToken);

            var productReadDto = product.Adapt<ProductReadDto>();

            await _cache.SetStringAsync($"product-{product.Id.Value}",
                JsonSerializer.Serialize(productReadDto),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

            return Result.Success();
        }

        return result;
    }

    public sealed record ProductNotFoundError(Guid ProductId) : Error("Product not found",
        $"Product to update with id: {ProductId} not found");

    public sealed record CategoryNotFoundError(Guid CategoryId) : Error("Category not found",
        $"Category not found, incorrect id:{CategoryId}");
}