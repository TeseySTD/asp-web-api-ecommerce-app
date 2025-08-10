using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Catalog.API.Http.Category.Requests;
using Catalog.API.Http.Product.Requests;
using Catalog.Application.UseCases.Products.Commands.UpdateProduct;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class UpdateProductTest : ApiTest
{
    public UpdateProductTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products";

    private HttpRequestMessage GenerateHttpRequest(Guid productId, UpdateProductRequest dto, Guid? sellerId = null,
        string role = "Seller")
    {
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{productId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["userId"] = sellerId?.ToString() ?? Guid.NewGuid().ToString(),
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = content;
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

    private UpdateProductRequest GenerateUpdateProductRequest(Guid? categoryId = null) =>
        new(
            "Updated Name",
            "Updated Description",
            12,
            333,
            Guid.NewGuid(),
            categoryId
        );


    [Fact]
    public async Task UpdateProduct_ValidData_ReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var dto = GenerateUpdateProductRequest();

        var request = GenerateHttpRequest(productId, dto, sellerId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify category was updated
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedProduct = await ApplicationDbContext.Products.FirstAsync(c => c.Id == product.Id);
        updatedProduct.Title.Value.Should().Be(dto.Title);
        updatedProduct.Description.Value.Should().Be(dto.Description);
        updatedProduct.Price.Value.Should().Be(dto.Price);
        updatedProduct.StockQuantity.Value.Should().Be(dto.Quantity);
    }

    [Fact]
    public async Task UpdateProduct_ProductNotInDb_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var dto = GenerateUpdateProductRequest();

        var request = GenerateHttpRequest(productId, dto);

        var expectedJson = MakeSystemErrorApiOutput(new UpdateProductCommandHandler.ProductNotFoundError(productId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task UpdateProduct_CategoryNotInDb_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var nonExistingCategoryId = Guid.NewGuid();
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var dto = GenerateUpdateProductRequest(nonExistingCategoryId);

        var request = GenerateHttpRequest(productId, dto, sellerId);

        var expectedJson =
            MakeSystemErrorApiOutput(new UpdateProductCommandHandler.CategoryNotFoundError(nonExistingCategoryId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task UpdateProduct_CustomerIsNotProductSeller_ReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var sellerId = Guid.NewGuid();
        var fakeSellerId = Guid.NewGuid();
        var product = CreateTestProduct(productId, sellerId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var dto = GenerateUpdateProductRequest();

        var request = GenerateHttpRequest(productId, dto, fakeSellerId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode); 
    }

    [Fact]
    public async Task UpdateProduct_Unautorized_ReturnsUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var dto = GenerateUpdateProductRequest(Guid.NewGuid());

        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{productId}", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_NotSeller_ReturnsForbidden()
    {
        // Arrange
        var dto = GenerateUpdateProductRequest(Guid.NewGuid());

        var request = GenerateHttpRequest(Guid.NewGuid(), dto, role: "Default");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}