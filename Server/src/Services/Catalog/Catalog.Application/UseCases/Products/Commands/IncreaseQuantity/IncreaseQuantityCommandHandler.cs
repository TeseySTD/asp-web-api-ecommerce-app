using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;

public class IncreaseQuantityCommandHandler : ICommandHandler<IncreaseQuantityCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;


    public IncreaseQuantityCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(IncreaseQuantityCommand request, CancellationToken cancellationToken)
    {
        var productId = ProductId.Create(request.Id).Value;
        var sellerId = SellerId.Create(request.SellerId).Value;
        
        var validationResult = await Result.Try()
            .Check(!await _context.Products.AnyAsync(p => p.Id == productId), 
                new ProductNotFoundError(request.Id))
            .DropIfFail()
            .CheckAsync(async () => !await _context.Products.AnyAsync(p => p.Id == productId && p.SellerId == sellerId),
                new CustomerMismatchError(request.SellerId))
            .BuildAsync();

        if (validationResult.IsFailure)
            return validationResult;

        var product = await _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == ProductId.Create(request.Id).Value, cancellationToken);
        product!.IncreaseQuantity((uint)request.Quantity);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.SetStringAsync($"product-{product.Id.Value}",
            JsonSerializer.Serialize(product.Adapt<ProductReadDto>()),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return Result.Success();
    }

    public sealed record ProductNotFoundError(Guid ProductId) : Error("Product not found",
        $"Product to update with id: {ProductId} not found");

    public sealed record CustomerMismatchError(Guid SellerId) : Error("You can`t increase quantity of this product!",
        $"Your id {SellerId} doesn’t match with seller’s id in product.");
}