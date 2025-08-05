using System.Text.Json;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.DeleteProductImage;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Catalog.Tests.Integration.Application.UseCases.Products.Commands;

public class DeleteProductImageCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public DeleteProductImageCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    private Product CreateTestProduct(Guid id) => Product.Create(
        ProductId.Create(id).Value,
        ProductTitle.Create("Test Product").Value,
        ProductDescription.Create("Test Description").Value,
        ProductPrice.Create(10).Value,
        SellerId.Create(Guid.NewGuid()).Value,
        null
    );

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentProductId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var cmd = new DeleteProductImageCommand(nonExistentProductId, imageId);
        var handler = new DeleteProductImageCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteProductImageCommandHandler.ProductNotFoundError);
    }

    [Fact]
    public async Task WhenImageNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var nonExistentImageId = Guid.NewGuid();
        var cmd = new DeleteProductImageCommand(productId, nonExistentImageId);
        var handler = new DeleteProductImageCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteProductImageCommandHandler.ImageNotFoundError);
    }

    [Fact]
    public async Task WhenImageExistsButNotBelongToProduct_ThenReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(33).Value;

        var orphanImage = Image.Create(
            FileName.Create("orphan.jpg").Value,
            ImageData.Create([1, 2, 3]).Value,
            ImageContentType.JPEG
        );

        ApplicationDbContext.Products.Add(product);
        ApplicationDbContext.Images.Add(orphanImage);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductImageCommand(productId, orphanImage.Id.Value);
        var handler = new DeleteProductImageCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteProductImageCommandHandler.ImageNotFoundError);
    }

    [Fact]
    public async Task WhenValidData_ThenRemovesImage_SavesContext_AndCachesDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(33).Value;

        var image1 = Image.Create(
            FileName.Create("image1.jpg").Value,
            ImageData.Create([1, 2, 3]).Value,
            ImageContentType.JPEG
        );

        var image2 = Image.Create(
            FileName.Create("image2.png").Value,
            ImageData.Create([4, 5, 6]).Value,
            ImageContentType.PNG
        );

        ApplicationDbContext.Products.Add(product);
        ApplicationDbContext.Images.AddRange(image1, image2);
        await ApplicationDbContext.SaveChangesAsync(default);

        product.AddImage(image1);
        product.AddImage(image2);
        await ApplicationDbContext.SaveChangesAsync(default);

        ConfigureMapster();
        var cmd = new DeleteProductImageCommand(productId, image1.Id.Value);
        var handler = new DeleteProductImageCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updatedProduct = await ApplicationDbContext.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .Include(p => p.Images)
            .FirstAsync(p => p.Id == product.Id);

        updatedProduct.Images.Should().HaveCount(1);
        updatedProduct.Images.Should().NotContain(i => i.Id == image1.Id);
        updatedProduct.Images.Should().Contain(i => i.Id == image2.Id);

        // Verify cache update
        var bytes = JsonSerializer.SerializeToUtf8Bytes(updatedProduct.Adapt<ProductReadDto>());
        await _cache.Received(1).SetAsync(
            $"product-{productId}",
            Arg.Is<byte[]>(b => b.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        );
    }
}