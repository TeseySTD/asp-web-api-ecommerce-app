using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
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
        var product = await _context.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == ProductId.Create(request.Id).Value, cancellationToken);

        var validationResult = Result.Try()
            .Check(product is null, new Error("Product not found", $"Product not found, incorrect id:{request.Id}"))
            .Check(product!.StockQuantity.Value < request.Quantity, new Error("Not enough quantity", "Not enough quantity in stock"))
            .Build();

        if (validationResult.IsSuccess)
        {
            product.DecreaseQuantity((uint)request.Quantity);
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
}