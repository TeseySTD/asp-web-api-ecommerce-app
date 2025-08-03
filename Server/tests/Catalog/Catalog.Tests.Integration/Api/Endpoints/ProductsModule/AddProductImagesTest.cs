using System.Net;
using System.Net.Http.Headers;
using Catalog.API.Endpoints;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class AddProductImagesTest : ApiTest
{
    public AddProductImagesTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products";

    private Product CreateTestProduct(Guid productId) => Product.Create(
        id: ProductId.Create(productId).Value,
        title: ProductTitle.Create("Test Product").Value,
        description: ProductDescription.Create("Test Product").Value,
        price: ProductPrice.Create(10).Value,
        null
    );

    private HttpRequestMessage GenereateRequest(Guid productId, MultipartFormDataContent form)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}/{productId}/images");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = form;
        return request;
    }

    [Fact]
    public async Task WhenValidImages_ThenReturnsOk()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();

        var imageContent1 = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        imageContent1.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        form.Add(imageContent1, "images", "test1.jpg");
        var imageContent2 = new ByteArrayContent(new byte[] { 5, 6, 7, 8 });
        imageContent2.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        form.Add(imageContent2, "images", "test2.png");

        var request = GenereateRequest(productId, form);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify images were added
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedProduct = await ApplicationDbContext.Products
            .Include(p => p.Images)
            .FirstAsync();
        updatedProduct.Images.Should().HaveCount(2);
    }

    [Fact]
    public async Task WhenNoImages_ThenReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        product.StockQuantity = StockQuantity.Create(1).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();
        var request = GenereateRequest(productId, form);

        // Act
        var response = await HttpClient.SendAsync(request);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenImagesAreExceededMaxCount_ThenReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);

        product.StockQuantity = StockQuantity.Create(1).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();
        for (int i = 0; i < Product.MaxImagesCount + 1; i++)
        {
            var img = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            img.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            form.Add(img, "images", $"test{i}.jpg");
        }

        var expectedJson = MakeSystemErrorApiOutput(new ProductModule.ImageCountOutOfRangeError());

        var request = GenereateRequest(productId, form);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenInvalidContentType_ThenReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = CreateTestProduct(productId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();
        var textContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        textContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        form.Add(textContent, "images", "test.txt");

        var expectedJson = MakeSystemErrorApiOutput(new ProductModule.InvalidImagesTypeError());

        var request = GenereateRequest(productId, form);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenProductDoesNotExist_ThenReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();

        using var form = new MultipartFormDataContent();
        var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        form.Add(imageContent, "images", "test1.jpg");

        var request = GenereateRequest(productId, form);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var productId = Guid.NewGuid();

        using var form = new MultipartFormDataContent();

        // Act
        var response = await HttpClient.PostAsync($"{RequestUrl}/{productId}/images", form);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenNotSeller_ThenReturnsForbidden()
    {
        // Arrange
        var productId = Guid.NewGuid();

        using var form = new MultipartFormDataContent();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}/{productId}/images");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Default"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = form;

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}