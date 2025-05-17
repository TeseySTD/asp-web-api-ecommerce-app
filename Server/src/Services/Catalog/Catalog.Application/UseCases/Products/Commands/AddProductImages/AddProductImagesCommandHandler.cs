using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Commands.AddProductImages;

public class AddProductImagesCommandHandler : ICommandHandler<AddProductImagesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public AddProductImagesCommandHandler(IApplicationDbContext context,
        IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(AddProductImagesCommand request, CancellationToken cancellationToken)
    {
        var productId = ProductId.Create(request.ProductId).Value;
        Product? product = null;
        var result = await Result.Try()
            .Check(!await _context.Products.AnyAsync(p => p.Id == productId),
                new("Product not found!", $"Product with id: {request.ProductId} was not found!"))
            .DropIfFail()
            .CheckAsync(async () =>
                {
                    product = await _context.Products
                        .Include(p => p.Category)
                            .ThenInclude(c => c.Images)
                        .Include(p => p.Images)
                        .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

                    return product!.Images.Count + request.Images.Count() > Product.MaxImagesCount;
                },
                new("Max images in product.",
                    $"Product with id: {request.ProductId} has maximum ({Product.MaxImagesCount}) images."))
            .BuildAsync();

        if (result.IsFailure)
            return result;


        foreach (var imageDto in request.Images)
        {
            var fileName = FileName.Create(imageDto.FileName).Value;
            var data = ImageData.Create(imageDto.Data).Value;
            var contentType = Enum.Parse<ImageContentType>(imageDto.ContentType, ignoreCase: true);

            var image = Image.Create(fileName, data, contentType);
            await _context.Images.AddAsync(image);
            product!.AddImage(image);
        }

        await _context.SaveChangesAsync(default);

        await _cache.SetStringAsync($"product-{product!.Id.Value}",
            JsonSerializer.Serialize(product.Adapt<ProductReadDto>()),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

        return Result.Success();
    }
}