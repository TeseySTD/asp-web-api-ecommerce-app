using System.Net;
using System.Net.Http.Headers;
using Catalog.Application.UseCases.Products.Commands.IncreaseQuantity;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class IncreaseQuantityTest : ApiTest
{
    public IncreaseQuantityTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products/increase-quantity";

    private HttpRequestMessage GenerateRequest(Guid productId, int quantityPlus, Guid? sellerId = null,
        string role = "Seller")
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{productId}/{quantityPlus}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["userId"] = sellerId?.ToString() ?? Guid.NewGuid().ToString(),
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private Product CreateTestProduct(Guid productId) => Product.Create(
        id: ProductId.Create(productId).Value,
        title: ProductTitle.Create("Test Product").Value,
        description: ProductDescription.Create("Test Product").Value,
        price: ProductPrice.Create(10).Value,
        sellerId: SellerId.Create(Guid.NewGuid()).Value,
        null
    );

    [Fact]
    public async Task IncreaseQuantity_ValidData_ReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialQuantity = 100;
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(initialQuantity).Value;
        var quantityPlus = 10;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId, quantityPlus, product.SellerId.Value);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Check quantity
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedProduct = await ApplicationDbContext.Products.FindAsync(ProductId.Create(productId).Value);
        updatedProduct!.StockQuantity.Value.Should().Be((uint)(initialQuantity + quantityPlus));
    }

    [Fact]
    public async Task IncreaseQuantity_ProductNotInDb_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityPlus = 10;

        var request = GenerateRequest(productId, quantityPlus);
        var expectedJson = MakeSystemErrorApiOutput(new IncreaseQuantityCommandHandler.ProductNotFoundError(productId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task IncreaseQuantity_CustomerIsNotProductSeller_ReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialQuantity = 100;
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(initialQuantity).Value;
        var quantityPlus = 10;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId, quantityPlus);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task IncreaseQuantity_Unathorized_ReturnsUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityPlus = 10;

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{productId}/{quantityPlus}", new StringContent(""));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task IncreaseQuantity_NotSeller_ReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityPlus = 10;

        var request = GenerateRequest(productId, quantityPlus, role: "Default");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}