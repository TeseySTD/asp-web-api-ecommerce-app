using Catalog.Application.UseCases.Products.Commands.DeleteProduct;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shared.Core.Auth;
using Shared.Messaging.Events.Product;

namespace Catalog.Tests.Integration.Application.UseCases.Products.Commands;

public class DeleteProductCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteProductCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
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
    public async Task Handle_ProductNotInDb_ReturnsProductNotFoundError()
    {
        // Arrange
        var nonExistentId = ProductId.Create(Guid.NewGuid()).Value;
        var sellerId = SellerId.Create(Guid.NewGuid()).Value;
        var role = UserRole.Default;
        var cmd = new DeleteProductCommand(nonExistentId, sellerId, role);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteProductCommandHandler.ProductNotFoundError);
    }

    [Fact]
    public async Task Handle_CustomerIsNotProductSeller_ReturnsCustomerMismatchError()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fakeSellerId = SellerId.Create(Guid.NewGuid()).Value;
        var product = CreateTestProduct(id);
        product.StockQuantity = StockQuantity.Create(100).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product.Id, fakeSellerId, UserRole.Default);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is DeleteProductCommandHandler.CustomerMismatchError);
    }


    [Fact]
    public async Task Handle_ProductExists_ShouldDeleteProductAndRemoveFromCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = CreateTestProduct(id);
        product.StockQuantity = StockQuantity.Create(100).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product.Id, product.SellerId, UserRole.Default);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product deleted from DB
        var deletedProduct = await ApplicationDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        deletedProduct.Should().BeNull();

        // Verify event is dispatched
        await _publishEndpoint.Received(1).Publish(Arg.Is<ProductDeletedEvent>(e => e.ProductId == product.Id.Value));

        // Verify cache removal
        await _cache.Received(1).RemoveAsync($"product-{id}");
    }

    [Fact]
    public async Task Handle_ValidDataCustomerIsAdminAndNotProductSeller_ShouldDeleteProductAndRemoveFromCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fakeSellerId = SellerId.Create(Guid.NewGuid()).Value;
        var product = CreateTestProduct(id);
        product.StockQuantity = StockQuantity.Create(100).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product.Id, fakeSellerId, UserRole.Admin);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product deleted from DB
        var deletedProduct = await ApplicationDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        deletedProduct.Should().BeNull();
        
        // Verify event is dispatched
        await _publishEndpoint.Received(1).Publish(Arg.Is<ProductDeletedEvent>(e => e.ProductId == product.Id.Value));
        
        // Verify cache removal
        await _cache.Received(1).RemoveAsync($"product-{id}");
    }

    [Fact]
    public async Task Handle_ProductHasImages_ShouldDeleteProductWithImages()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = CreateTestProduct(id);
        product.StockQuantity = StockQuantity.Create(50).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product.Id, product.SellerId, UserRole.Default);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product and related data deleted from DB
        var deletedProduct = await ApplicationDbContext.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == product.Id);
        deletedProduct.Should().BeNull();

        // Verify event is dispatched
        await _publishEndpoint.Received(1).Publish(Arg.Is<ProductDeletedEvent>(e => e.ProductId == product.Id.Value));
        
        // Verify cache removal
        await _cache.Received(1).RemoveAsync($"product-{id}");
    }

    [Fact]
    public async Task Handle_MultipleProducts_ShouldDeleteOnlySpecifiedProduct()
    {
        // Arrange
        var product1Id = Guid.NewGuid();
        var product1 = CreateTestProduct(product1Id);
        product1.StockQuantity = StockQuantity.Create(100).Value;

        var product2Id = Guid.NewGuid();
        var product2 = CreateTestProduct(product2Id);
        product2.StockQuantity = StockQuantity.Create(50).Value;

        ApplicationDbContext.Products.AddRange(product1, product2);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteProductCommand(product1.Id, product1.SellerId, UserRole.Default);
        var handler = new DeleteProductCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

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


        // Verify event is dispatched
        await _publishEndpoint.Received(1).Publish(Arg.Is<ProductDeletedEvent>(e => e.ProductId == product1.Id.Value));
        await _publishEndpoint.DidNotReceive().Publish(Arg.Is<ProductDeletedEvent>(e => e.ProductId == product2.Id.Value));
        
        // Verify cache removal
        await _cache.Received(1).RemoveAsync($"product-{product1Id}");
        await _cache.DidNotReceive().RemoveAsync($"product-{product2Id}");
    }
}