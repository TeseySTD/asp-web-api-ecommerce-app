using System.Text.Json;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.DecreaseQuantity;
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

public class DecreaseQuantityCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public DecreaseQuantityCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var cmd = new DecreaseQuantityCommand(nonExistentId, 5);
        var handler = new DecreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DecreaseQuantityCommandHandler.ProductNotFoundError);
    }

    [Fact]
    public async Task WhenNotEnoughQuantity_ThenReturnsFailureResult()
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
            ProductPrice.Create(10).Value,
            CategoryId.Create(categoryId).Value
        );
        product.StockQuantity = StockQuantity.Create(5).Value; // Only 5 in stock
        
        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DecreaseQuantityCommand(id, 10); // Trying to decrease by 10
        var handler = new DecreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DecreaseQuantityCommandHandler.NotEnoughtQuantityError);
    }

    [Fact]
    public async Task WhenValidData_ThenDecreasesQuantity_SavesContext_AndCachesDto()
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
        uint initQuantity = 100;
        product.StockQuantity = StockQuantity.Create(initQuantity).Value;
        
        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        ConfigureMapster();
        uint minusQuantity = 30;
        var cmd = new DecreaseQuantityCommand(id, (int)minusQuantity);
        var handler = new DecreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify quantity decreased in DB
        var updatedProduct = await ApplicationDbContext.Products
            .Include(p => p.Category)
                .ThenInclude(c => c.Images)
            .Include(p => p.Images)
            .FirstAsync(p => p.Id == product.Id);
        
        updatedProduct.StockQuantity.Value.Should().Be(initQuantity - minusQuantity); 

        // Verify cache update
        var bytes = JsonSerializer.SerializeToUtf8Bytes(updatedProduct.Adapt<ProductReadDto>());
        await _cache.Received(1).SetAsync(
            $"product-{id}",
            Arg.Is<byte[]>(b => b.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        );
    }
}
