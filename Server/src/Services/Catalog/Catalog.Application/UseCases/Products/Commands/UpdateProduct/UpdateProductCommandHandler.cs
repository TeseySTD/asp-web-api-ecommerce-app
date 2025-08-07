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
        var productId = ProductId.Create(request.Value.Id).Value;
        var sellerId = SellerId.Create(request.CurrentSellerId).Value;
        var categoryId = CategoryId.Create(request.Value.CategoryId ?? Guid.Empty).Value;
        
        var validationResul = await ValidateDataAsync(request, productId, categoryId, sellerId);

        if (validationResul.IsFailure)
            return validationResul;
            
        var product = await _context.Products
            .Where(p => p.Id == ProductId.Create(request.Value.Id).Value)
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(cancellationToken);

        var category = request.Value.CategoryId.HasValue
            ? await _context.Categories
                .Include(c => c.Images)
                .FirstOrDefaultAsync(p => p.Id == CategoryId.Create((Guid)request.Value.CategoryId).Value, cancellationToken)
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

    private async Task<Result> ValidateDataAsync(UpdateProductCommand request, ProductId productId, CategoryId categoryId, SellerId sellerId)
    {
        var result = await Result.Try()
            .Check(!await _context.Products.AnyAsync(p => p.Id == productId), 
                new ProductNotFoundError(productId.Value))
            .CheckIfAsync(
                request.Value.CategoryId != null,
                async () => !await _context.Categories.AnyAsync(p => p.Id == categoryId),
                new CategoryNotFoundError(request.Value.CategoryId ?? Guid.Empty))
            .DropIfFail()
            .CheckAsync(async () => !await _context.Products.AnyAsync(p => p.Id == productId && p.SellerId == sellerId),
                new CustomerMismatchError(sellerId.Value))
            .BuildAsync();
        return result;
    }

    public sealed record ProductNotFoundError(Guid ProductId) : Error("Product not found",
        $"Product to update with id: {ProductId} not found");

    public sealed record CategoryNotFoundError(Guid CategoryId) : Error("Category not found",
        $"Category not found, incorrect id:{CategoryId}");
    public sealed record CustomerMismatchError(Guid SellerId) : Error("You can`t update this product!",
        $"Your id {SellerId} doesn’t match with seller’s id in product.");
}