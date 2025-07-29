using System.Text.Json;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Queries.GetProductById;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Application.UseCases.Products.Queries;

public class GetProductByIdQueryHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;
    private readonly GetProductByIdQueryHandler _handler;

    public GetProductByIdQueryHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
        _handler = new GetProductByIdQueryHandler(ApplicationDbContext, _cache);
        ConfigureMapster();
    }

    private async Task<Product> GetProduct()
    {
        var product = Product.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("Title").Value,
            ProductDescription.Create("Description").Value,
            ProductPrice.Create(9.9m).Value,
            null
        );
        product.StockQuantity = StockQuantity.Create(3).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);
        return product;
    }

    [Fact]
    public async Task WhenCached_ThenReturnsFromCache()
    {
        // Arrange
        var product = await GetProduct();
        var id = product.Id.Value;
        var dto = new ProductReadDto(
            id,
            "Title",
            "Description",
            1m,
            10,
            Array.Empty<string>(),
            null
        );
        var bytes = JsonSerializer.SerializeToUtf8Bytes(dto);
        _cache.GetAsync($"product-{id}").Returns(bytes);

        var query = new GetProductByIdQuery(ProductId.Create(id).Value);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task WhenNotCachedAndExists_ThenCachesAndReturns()
    {
        // Arrange
        var product = await GetProduct();
        _cache.GetAsync($"product-{product.Id.Value}").Returns([]);

        var query = new GetProductByIdQuery(product.Id);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _cache.Received(1).SetAsync(
            $"product-{product.Id.Value}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>()
        );
    }

    [Fact]
    public async Task WhenNotCachedAndNotExists_ThenReturnsNotFound()
    {
        // Arrange
        _cache.GetAsync($"product-{Guid.NewGuid()}").Returns([]);

        var id = ProductId.Create(Guid.NewGuid()).Value;
        var query = new GetProductByIdQuery(id); 
        
        //Act
        var result = await _handler.Handle(query, default);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == Error.NotFound);
    }
}