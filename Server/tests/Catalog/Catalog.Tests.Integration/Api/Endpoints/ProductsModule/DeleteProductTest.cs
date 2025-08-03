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

    private HttpRequestMessage GenerateRequest(Guid productId, string role = "Seller")
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{productId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = Product.Create(
            id: ProductId.Create(productId).Value,
            title: ProductTitle.Create("Test Product").Value,
            description: ProductDescription.Create("Test Product").Value,
            price: ProductPrice.Create(10).Value,
            null
        );
        product.StockQuantity = StockQuantity.Create(10).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(productId);

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

        var request = GenerateRequest(productId);

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
    public async Task WhenNotSeller_ThenReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var request = GenerateRequest(productId, "Default");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}