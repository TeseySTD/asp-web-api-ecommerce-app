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

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var cmd = new IncreaseQuantityCommand(nonExistentId, 5);
        var handler = new IncreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is IncreaseQuantityCommandHandler.ProductNotFoundError);
    }

    [Fact]
    public async Task WhenValidData_ThenIncreasesQuantity_SavesContext_AndCachesDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Test").Value,
            CategoryDescription.Create("Test Description").Value
        );
        var prodId = Guid.NewGuid();
        var product = Product.Create(
            ProductId.Create(prodId).Value,
            ProductTitle.Create("Title").Value,
            ProductDescription.Create("Descripction").Value,
            ProductPrice.Create(10m).Value,
            category.Id
        );
        product.StockQuantity = StockQuantity.Create(10).Value;

        ApplicationDbContext.Products.Add(product);
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var increaseBy = 7u;
        var cmd = new IncreaseQuantityCommand(prodId, (int)increaseBy);
        ConfigureMapster();
        var handler = new IncreaseQuantityCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify DB update
        var updated = await ApplicationDbContext.Products
            .FirstOrDefaultAsync(p => p.Id == ProductId.Create(prodId).Value);
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