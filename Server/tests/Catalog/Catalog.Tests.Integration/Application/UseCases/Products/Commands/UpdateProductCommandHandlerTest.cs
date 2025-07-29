using System.Text.Json;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.UpdateProduct;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Catalog.Tests.Integration.Application.UseCases.Products.Commands;

public class UpdateProductCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public UpdateProductCommandHandlerTest(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var dto = new ProductUpdateDto(
            Id: nonExistentId,
            Title: "Any",
            Description: "Any",
            Price: 5m,
            CategoryId: null,
            Quantity: 1
        );
        var cmd = new UpdateProductCommand(dto);
        var handler = new UpdateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UpdateProductCommandHandler.ProductNotFoundError);
    }

    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var product = Product.Create(
            ProductId.Create(productId).Value,
            ProductTitle.Create("Title").Value,
            ProductDescription.Create("Description").Value,
            ProductPrice.Create(1m).Value,
            null
        );
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var invalidCategoryId = Guid.NewGuid();
        var dto = new ProductUpdateDto(
            Id: productId,
            Title: "Updated",
            Description: "Updated description",
            Price: 2m,
            CategoryId: invalidCategoryId,
            Quantity: 2
        );
        var cmd = new UpdateProductCommand(dto);
        var handler = new UpdateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UpdateProductCommandHandler.CategoryNotFoundError);
    }

    [Fact]
    public async Task WhenValidData_ThenUpdatesProduct_SavesContext_AndCachesDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Old Category").Value,
            CategoryDescription.Create("Old Description").Value
        );
        var prodId = Guid.NewGuid();
        var product = Product.Create(
            ProductId.Create(prodId).Value,
            ProductTitle.Create("Old Title").Value,
            ProductDescription.Create("Old Description").Value,
            ProductPrice.Create(10m).Value,
            category.Id
        );
        product.StockQuantity = StockQuantity.Create(10).Value;
        var newCategoryId = Guid.NewGuid();
        var newCategory = Category.Create(
            CategoryId.Create(newCategoryId).Value,
            CategoryName.Create("New Category").Value,
            CategoryDescription.Create("New Description").Value
        );

        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Categories.Add(newCategory);
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var dto = new ProductUpdateDto(
            Id: prodId,
            Title: "New Title",
            Description: "New Description",
            Price: 20.5m,
            CategoryId: newCategoryId,
            Quantity: 25
        );

        ConfigureMapster();
        var cmd = new UpdateProductCommand(dto);
        var handler = new UpdateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify DB update
        var updated = await ApplicationDbContext.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == ProductId.Create(prodId).Value);
        updated.Should().NotBeNull();
        updated.Title.Value.Should().Be(dto.Title);
        updated.Description.Value.Should().Be(dto.Description);
        updated.Price.Value.Should().Be(dto.Price);
        updated.CategoryId!.Value.Should().Be((Guid)dto.CategoryId!);
        updated.StockQuantity.Value.Should().Be(dto.Quantity);

        // Verify cache set
        var expectedDto = updated.Adapt<ProductReadDto>();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(expectedDto);
        await _cache.Received(1).SetAsync(
            $"product-{updated.Id.Value}",
            Arg.Is<byte[]>(s => s.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>());
    }
}