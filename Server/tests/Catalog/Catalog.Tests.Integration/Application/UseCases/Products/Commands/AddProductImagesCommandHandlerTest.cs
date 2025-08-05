using System.Text.Json;
using Catalog.Application.Dto.Image;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.AddProductImages;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Catalog.Tests.Integration.Application.UseCases.Products.Commands;

public class AddProductImagesCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public AddProductImagesCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    private Product CreateTestProduct(Guid id) => Product.Create(
        ProductId.Create(id).Value,
        ProductTitle.Create("Test Title").Value,
        ProductDescription.Create("Test Description").Value,
        ProductPrice.Create(10).Value,
        SellerId.Create(Guid.NewGuid()).Value,
        categoryId: null
    );

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var cmd = new AddProductImagesCommand(Guid.NewGuid(), Guid.NewGuid(),
            new[] { new ImageDto("f.jpg", "Png", [1]) });
        var handler = new AddProductImagesCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is AddProductImagesCommandHandler.ProductNotFoundError);
    }

    [Fact]
    public async Task WhenProductSellerIsNotCustomer_ThenReturnsFailureResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fakeSellerId = Guid.NewGuid();
        var product = CreateTestProduct(id);
        product.StockQuantity = StockQuantity.Create(100).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var images = new[]
        {
            new ImageDto("a.jpg", "Png", new byte[] { 1 }),
            new ImageDto("b.jpg", "Jpeg", new byte[] { 2 })
        };

        ConfigureMapster();
        var cmd = new AddProductImagesCommand(id, fakeSellerId, images);
        var handler = new AddProductImagesCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is AddProductImagesCommandHandler.CustomerMismatchError);
    }

    [Fact]
    public async Task WhenTooMuchImages_ThenReturnsFailureResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = CreateTestProduct(id);
        product.StockQuantity = StockQuantity.Create(100).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var images = Enumerable.Range(1, Product.MaxImagesCount + 1).Select(i => new ImageDto(i.ToString(), "Png", [1]))
            .ToList();

        var cmd = new AddProductImagesCommand(id, product.SellerId.Value, images);
        var handler = new AddProductImagesCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is AddProductImagesCommandHandler.MaxImagesError);
    }

    [Fact]
    public async Task WhenValidImages_ThenAddsImages_SavesContext_AndCachesDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = CreateTestProduct(id);
        product.StockQuantity = StockQuantity.Create(100).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var images = new[]
        {
            new ImageDto("a.jpg", "Png", new byte[] { 1 }),
            new ImageDto("b.jpg", "Jpeg", new byte[] { 2 })
        };

        ConfigureMapster();
        var cmd = new AddProductImagesCommand(id, product.SellerId.Value, images);
        var handler = new AddProductImagesCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify images added in DB and linked to product
        var updated = await ApplicationDbContext.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .Include(p => p.Images)
            .FirstAsync(p => p.Id == product.Id);
        updated.Images.Should().HaveCount(images.Length);

        // Verify cache update
        var bytes = JsonSerializer.SerializeToUtf8Bytes(updated.Adapt<ProductReadDto>());
        await _cache.Received(1).SetAsync(
            $"product-{id}",
            Arg.Is<byte[]>(b => b.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        );
    }
}