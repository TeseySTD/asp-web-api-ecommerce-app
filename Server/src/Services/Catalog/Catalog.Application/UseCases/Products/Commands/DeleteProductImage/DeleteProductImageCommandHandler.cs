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
            return new ProductNotFoundError(request.ProductId);
        if (image is null || !product.Images.Any(i => i.Id == imageId))
            return new ImageNotFoundError(request.ImageId, request.ProductId);
        
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
    
    public sealed record ProductNotFoundError(Guid ProductId) : Error("Product not found, incorrect id",
                                                                                          $"Product not found, incorrect id:{ProductId}");
    public sealed record ImageNotFoundError(Guid ImageId, Guid ProductId) : Error("Image not found",
                                                                                                 $"Image not found, incorrect id:{ImageId} or not belong to product with id:{ProductId}");
}