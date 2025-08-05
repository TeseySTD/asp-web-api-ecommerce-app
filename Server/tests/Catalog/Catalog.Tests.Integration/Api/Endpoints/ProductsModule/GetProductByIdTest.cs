using System.Net;
using System.Net.Http.Json;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class GetProductByIdTest : ApiTest
{
    public GetProductByIdTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products";

    [Fact]
    public async Task WhenProductExists_ThenReturnsOk()
    {
        // Arrange
        var product = Product.Create(
            title: ProductTitle.Create("Test Product1").Value,
            description: ProductDescription.Create("Test Product1").Value,
            price: ProductPrice.Create(10).Value,
            sellerId: SellerId.Create(Guid.NewGuid()).Value,
            null
        );
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{product.Id.Value}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ProductReadDto>();
        Assert.NotNull(result);
        result.Id.Should().Be(product.Id.Value);
        result.Title.Should().Be(product.Title.Value);
        result.Description.Should().Be(product.Description.Value);
    }

    [Fact]
    public async Task WhenProductDoesNotExist_ThenReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var expectedJson = MakeSystemErrorApiOutput(Error.NotFound);
        
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{nonExistentId}");
        var actualJson = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}