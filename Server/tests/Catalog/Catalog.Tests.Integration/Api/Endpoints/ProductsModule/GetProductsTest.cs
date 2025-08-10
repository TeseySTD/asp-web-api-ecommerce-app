using System.Net;
using System.Net.Http.Json;
using Catalog.API.Http.Product.Responses;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class GetProductsTest : ApiTest
{
    public GetProductsTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products";

    [Fact]
    public async Task GetProducts_ProductsInDb_ReturnsOk()
    {
        // Arrange
        var product1 = Product.Create(
            title: ProductTitle.Create("Test Product1").Value,
            description: ProductDescription.Create("Test Product1").Value,
            price: ProductPrice.Create(10).Value,
            sellerId: SellerId.Create(Guid.NewGuid()).Value,
            null
        );
        product1.StockQuantity = StockQuantity.Create(1).Value;
        var product2 = Product.Create(
            title: ProductTitle.Create("Test Product2").Value,
            description: ProductDescription.Create("Test Product2").Value,
            price: ProductPrice.Create(10).Value,
            sellerId: SellerId.Create(Guid.NewGuid()).Value,
            null
        );
        product2.StockQuantity = StockQuantity.Create(1).Value;
        Product[] products = [product1, product2];

        ApplicationDbContext.Products.AddRange(products);
        await ApplicationDbContext.SaveChangesAsync();

        var page = 0;
        var pageSize = 10;

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?page={page}&pageSize={pageSize}&minPrice={product1.Price.Value}&maxPrice={product2.Price.Value}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<GetProductsResponse>();
        Assert.NotNull(result);
        foreach (var p in products)
        {
            result.Products.Data.Should().Contain(d =>
                d.Id == p.Id.Value &&
                d.Title == p.Title.Value &&
                d.Description == p.Description.Value &&
                d.Price == p.Price.Value
            );
        }
    }

    [Fact]
    public async Task GetProducts_ProductsNotInDb_ReturnsNotFound()
    {
        // Arrange 
        var page = 0;
        var pageSize = 10;

        var expectedJson = MakeSystemErrorApiOutput(Error.NotFound);

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?page={page}&pageSize={pageSize}");
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}