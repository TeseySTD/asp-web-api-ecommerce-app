using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Commands.DecreaseQuantity;

public class DecreaseQuantityCommandHandler : ICommandHandler<DecreaseQuantityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public DecreaseQuantityCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(DecreaseQuantityCommand request, CancellationToken cancellationToken)
    {
        Product? product = null;
        var productId = ProductId.Create(request.Id).Value;
        var sellerId = SellerId.Create(request.SellerId).Value;

        var validationResult = await Result.Try()
            .Check(!await _context.Products.AnyAsync(p => p.Id == productId), new ProductNotFoundError(request.Id))
            .DropIfFail()
            .CheckAsync(async () => !await _context.Products.AnyAsync(p => p.Id == productId && p.SellerId == sellerId), 
                new CustomerMismatchError(request.SellerId))
            .CheckAsync(async () =>
            {
                product = await _context.Products
                    .Include(p => p.Category)
                        .ThenInclude(c => c.Images)
                    .Include(p => p.Images)
                    .FirstOrDefaultAsync(p => p.Id == ProductId.Create(request.Id).Value, cancellationToken);
                return product!.StockQuantity.Value < request.Quantity;
            }, new NotEnoughtQuantityError())
            .BuildAsync();

        if (validationResult.IsSuccess)
        {
            product!.DecreaseQuantity((uint)request.Quantity);
            await _context.SaveChangesAsync(cancellationToken);

            await _cache.SetStringAsync($"product-{product.Id.Value}",
                JsonSerializer.Serialize(product.Adapt<ProductReadDto>()),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });
        }

        return validationResult;
    }

    public sealed record ProductNotFoundError(Guid ProductId)
        : Error($"Product not found", $"Product not found, incorrect id:{ProductId}");
    public sealed record NotEnoughtQuantityError() : Error($"Not enough quantity", $"Not enough quantity in stock");
    public sealed record CustomerMismatchError(Guid SellerId) : Error("You can`t decrease quantity of this product!",
        $"Your id {SellerId} doesn’t match with seller’s id in product.");
}