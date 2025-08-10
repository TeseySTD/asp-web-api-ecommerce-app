using System.Text.Json;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;
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

public class IncreaseQuantityCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public IncreaseQuantityCommandHandlerTest(DatabaseFixture databaseFixture)
        : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    private Product CreateTestProduct(Guid productId) => Product.Create(
        ProductId.Create(productId).Value,
        ProductTitle.Create("Title").Value,
        ProductDescription.Create("Descripction").Value,
        ProductPrice.Create(10m).Value,
        SellerId.Create(Guid.NewGuid()).Value,
        null
    );

    [Fact]
    public async Task Handle_ProductNotInDb_ReturnsProductNotFoundError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var cmd = new IncreaseQuantityCommand(nonExistentId, Guid.NewGuid(), 5);
        var handler = new IncreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is IncreaseQuantityCommandHandler.ProductNotFoundError);
    }

    [Fact] public async Task Handle_ProductSellerIsNotCustomer_ReturnsCustomerMismatchError()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var fakeSellerId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(10).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var increaseBy = 7u;
        var cmd = new IncreaseQuantityCommand(productId, fakeSellerId, (int)increaseBy);
        ConfigureMapster();
        var handler = new IncreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is IncreaseQuantityCommandHandler.CustomerMismatchError);
    }

    [Fact]
    public async Task Handle_ValidData_ShouldIncreaseQuantityAndCacheDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(10).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var increaseBy = 7u;
        var cmd = new IncreaseQuantityCommand(productId, product.SellerId.Value, (int)increaseBy);
        ConfigureMapster();
        var handler = new IncreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify DB update
        var updated = await ApplicationDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == ProductId.Create(productId).Value);
        updated.Should().NotBeNull();
        updated.StockQuantity.Value.Should().Be(10 + increaseBy);

        // Verify cache set
        var expectedDto = updated.Adapt<ProductReadDto>();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(expectedDto);
        await _cache.Received(1).SetAsync(
            $"product-{updated.Id.Value}",
            Arg.Is<byte[]>(b => b.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        );
    }
}