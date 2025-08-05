using System.Net;
using System.Net.Http.Headers;
using Catalog.Application.UseCases.Products.Commands.DeleteProduct;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class DeleteProductTest : ApiTest
{
    public DeleteProductTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products";

    private HttpRequestMessage GenerateRequest(Guid productId, Guid? sellerId = null, string role = "Seller")
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{productId}");
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
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(10).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId, sellerId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var request = GenerateRequest(productId, Guid.NewGuid());

        var expectedJson = MakeSystemErrorApiOutput(new DeleteProductCommandHandler.ProductNotFoundError(productId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{productId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerIsNotProductSeller_ThenReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var fakeSellerId = Guid.NewGuid();
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(10).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId, fakeSellerId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerIsNotProductSellerButIsAdminAndValidData_ThenReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var fakeSellerId = Guid.NewGuid();
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(10).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId, fakeSellerId, role: "Admin");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WhenNotSeller_ThenReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = GenerateRequest(productId, role: "Default");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}