using Catalog.Application.UseCases.Products.Commands.DeleteProduct;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Catalog.Tests.Integration.Application.UseCases.Products.Commands;

public class DeleteProductCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public DeleteProductCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentId = ProductId.Create(Guid.NewGuid()).Value;
        var cmd = new DeleteProductCommand(nonExistentId);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteProductCommandHandler.ProductNotFoundError);
    }

    [Fact]
    public async Task WhenProductExists_ThenDeletesProduct_AndRemovesFromCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Test Category").Value,
            CategoryDescription.Create("Test Description").Value
        );
        
        var product = Product.Create(
            ProductId.Create(id).Value,
            ProductTitle.Create("Test Product").Value,
            ProductDescription.Create("Test Description").Value,
            ProductPrice.Create(25.99m).Value,
            CategoryId.Create(categoryId).Value
        );
        product.StockQuantity = StockQuantity.Create(100).Value;
        
        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product.Id);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product deleted from DB
        var deletedProduct = await ApplicationDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        deletedProduct.Should().BeNull();

        // Verify cache removal
        await _cache.Received(1).RemoveAsync($"product-{id}");
    }

    [Fact]
    public async Task WhenProductHasImages_ThenDeletesProductWithImages()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Test Category").Value,
            CategoryDescription.Create("Test Description").Value
        );
        
        var product = Product.Create(
            ProductId.Create(id).Value,
            ProductTitle.Create("Test Product").Value,
            ProductDescription.Create("Test Description").Value,
            ProductPrice.Create(15.50m).Value,
            CategoryId.Create(categoryId).Value
        );
        product.StockQuantity = StockQuantity.Create(50).Value;
        
        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product.Id);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product and related data deleted from DB
        var deletedProduct = await ApplicationDbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        deletedProduct.Should().BeNull();

        // Verify cache removal
        await _cache.Received(1).RemoveAsync($"product-{id}");
    }

    [Fact]
    public async Task WhenMultipleProductsExist_ThenDeletesOnlySpecifiedProduct()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Test Category").Value,
            CategoryDescription.Create("Test Description").Value
        );

        var product1Id = Guid.NewGuid();
        var product1 = Product.Create(
            ProductId.Create(product1Id).Value,
            ProductTitle.Create("Product 1").Value,
            ProductDescription.Create("Description 1").Value,
            ProductPrice.Create(10).Value,
            CategoryId.Create(categoryId).Value
        );
        product1.StockQuantity = StockQuantity.Create(100).Value;
        
        var product2Id = Guid.NewGuid();
        var product2 = Product.Create(
            ProductId.Create(product2Id).Value,
            ProductTitle.Create("Product 2").Value,
            ProductDescription.Create("Description 2").Value,
            ProductPrice.Create(20).Value,
            CategoryId.Create(categoryId).Value
        );
        product2.StockQuantity = StockQuantity.Create(50).Value;

        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Products.AddRange(product1, product2);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product1.Id);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var deletedProduct = await ApplicationDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product1.Id);
        deletedProduct.Should().BeNull();

        var remainingProduct = await ApplicationDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product2.Id);
        remainingProduct.Should().NotBeNull();

        await _cache.Received(1).RemoveAsync($"product-{product1Id}");
        await _cache.DidNotReceive().RemoveAsync($"product-{product2Id}");
    }
}
