using System.Net;
using System.Net.Http.Headers;
using Catalog.Application.UseCases.Products.Commands.DecreaseQuantity;
using Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class DecreaseQuantityTest : ApiTest
{
    public DecreaseQuantityTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products/decrease-quantity";

    private HttpRequestMessage GenerateRequest(Guid productId, int quantity, Guid? sellerId = null,
        string role = "Seller")
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{productId}/{quantity}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["userId"] = sellerId?.ToString() ?? Guid.NewGuid().ToString(),
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private Product CreateTestProduct(Guid productId, Guid sellerId) => Product.Create(
        id: ProductId.Create(productId).Value,
        title: ProductTitle.Create("Test Product").Value,
        description: ProductDescription.Create("Test Product").Value,
        price: ProductPrice.Create(10).Value,
        sellerId: SellerId.Create(sellerId).Value,
        null
    );

    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var initialQuantity = 100;
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(initialQuantity).Value;
        var quantityMinus = 10;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId, quantityMinus, sellerId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Check quantity
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedProduct = await ApplicationDbContext.Products.FindAsync(ProductId.Create(productId).Value);
        updatedProduct!.StockQuantity.Value.Should().Be((uint)(initialQuantity - quantityMinus));
    }

    [Fact]
    public async Task WhenProductDoesNotExist_ThenReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityMinus = 10;

        var request = GenerateRequest(productId, quantityMinus);
        var expectedJson = MakeSystemErrorApiOutput(new DecreaseQuantityCommandHandler.ProductNotFoundError(productId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenCustomerIsNotProductSeller_ThenReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var initialQuantity = 100;
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(initialQuantity).Value;
        var quantityMinus = 10;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId, quantityMinus);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WhenUnathorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityMinus = 10;

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{productId}/{quantityMinus}", new StringContent(""));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenNotSeller_ThenReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityMinus = 10;

        var request = GenerateRequest(productId, quantityMinus, role: "Default");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}