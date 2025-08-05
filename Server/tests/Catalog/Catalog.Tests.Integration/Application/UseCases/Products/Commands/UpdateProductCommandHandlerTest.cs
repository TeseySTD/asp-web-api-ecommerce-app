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

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("Test Category").Value,
        CategoryDescription.Create("Test Description").Value
    );

    private Product CreateTestProduct(Guid id, CategoryId? categoryId = null, Guid? sellerId = null) => Product.Create(
        ProductId.Create(id).Value,
        ProductTitle.Create("Test Product").Value,
        ProductDescription.Create("Test Description").Value,
        ProductPrice.Create(10).Value,
        SellerId.Create(sellerId ?? Guid.NewGuid()).Value,
        categoryId
    );

    private ProductUpdateDto GenerateProductUpdateDto(Guid productId, Guid sellerId, Guid? categoryId = null) => new(
        Id: productId,
        Title: "Updated",
        Description: "Updated description",
        Price: 2m,
        SellerId: sellerId,
        CategoryId: categoryId,
        Quantity: 2
    );

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var dto = GenerateProductUpdateDto(nonExistentId, sellerId: sellerId);
        var cmd = new UpdateProductCommand(sellerId, dto);
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
        var sellerId = Guid.NewGuid();
        var product = CreateTestProduct(productId, sellerId: sellerId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var invalidCategoryId = Guid.NewGuid();
        var dto = GenerateProductUpdateDto(productId, sellerId, invalidCategoryId);
        var cmd = new UpdateProductCommand(sellerId, dto);
        var handler = new UpdateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UpdateProductCommandHandler.CategoryNotFoundError);
    }


    [Fact]
    public async Task WhenProductSellerIsNotCustomer_ThenReturnsFailureResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        var prodId = Guid.NewGuid();
        var product = CreateTestProduct(prodId, category.Id, sellerId);
        product.StockQuantity = StockQuantity.Create(10).Value;

        var newCategoryId = Guid.NewGuid();
        var newCategory = CreateTestCategory(newCategoryId);
        var newSellerId = Guid.NewGuid();

        ApplicationDbContext.Categories.AddRange(category, newCategory);
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var dto = GenerateProductUpdateDto(prodId, newSellerId, newCategoryId);

        ConfigureMapster();
        var cmd = new UpdateProductCommand(newSellerId, dto);
        var handler = new UpdateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UpdateProductCommandHandler.CustomerMismatchError);
    }

    [Fact]
    public async Task WhenValidData_ThenUpdatesProductSavesContextAndCachesDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        var prodId = Guid.NewGuid();
        var product = CreateTestProduct(prodId, category.Id, sellerId);
        product.StockQuantity = StockQuantity.Create(10).Value;

        var newCategoryId = Guid.NewGuid();
        var newCategory = CreateTestCategory(newCategoryId);
        var newSellerId = Guid.NewGuid();

        ApplicationDbContext.Categories.AddRange(category, newCategory);
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var dto = GenerateProductUpdateDto(prodId, newSellerId, newCategoryId);

        ConfigureMapster();
        var cmd = new UpdateProductCommand(sellerId, dto);
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
        updated.SellerId.Value.Should().Be(dto.SellerId);
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