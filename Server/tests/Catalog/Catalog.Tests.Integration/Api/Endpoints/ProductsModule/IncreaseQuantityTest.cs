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

    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var initialQuantity = 100;
        var product = Product.Create(
            id: ProductId.Create(productId).Value,
            title: ProductTitle.Create("Test Product").Value,
            description: ProductDescription.Create("Test Product").Value,
            price: ProductPrice.Create(10).Value,
            null
        );
        product.StockQuantity = StockQuantity.Create(initialQuantity).Value;
        var quantityPlus = 10;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{productId}/{quantityPlus}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

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
    public async Task WhenProductDoesNotExist_ThenReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityPlus = 10;

        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{productId}/{quantityPlus}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var expectedJson = MakeSystemErrorApiOutput(new IncreaseQuantityCommandHandler.ProductNotFoundError(productId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenUnathorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityPlus = 10;

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{productId}/{quantityPlus}", new StringContent(""));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact] public async Task WhenNotSeller_ThenReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var quantityPlus = 10;

        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{productId}/{quantityPlus}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Default"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}