using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Commands.DeleteProductImage;

public class DeleteProductImageCommandHandler : ICommandHandler<DeleteProductImageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;
    public DeleteProductImageCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
    {
        var (productId, imageId) = (ProductId.Create(request.ProductId).Value, ImageId.Create(request.ImageId).Value);
        
        var product = await _context.Products
            .Include(c => c.Category)
                .ThenInclude(c => c.Images)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        var image = await _context.Images
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == imageId, cancellationToken);
        
        if (product is null)
            return new Error("Product not found", $"Product not found, incorrect id:{request.ProductId}");
        if (image is null || !product.Images.Any(i => i.Id == imageId))
            return new Error("Image not found",
                $"Image not found, incorrect id:{request.ImageId} or not belong to product with id:{request.ProductId}");
        
        product.RemoveImage(imageId);
        await _context.SaveChangesAsync(cancellationToken);
        
        await _cache.SetStringAsync($"product-{product.Id.Value}",
            JsonSerializer.Serialize(product.Adapt<ProductReadDto>()),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
        
        return Result.Success();
    }
}