using System.Net;
using System.Net.Http.Headers;
using Catalog.Application.UseCases.Products.Commands.DeleteProductImage;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class DeleteProductImageTest : ApiTest
{
    public DeleteProductImageTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products";

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
        product.StockQuantity = StockQuantity.Create(1).Value;
        var image = Image.Create(
            FileName.Create("test.jpg").Value,
            ImageData.Create(new byte[] { 1, 2, 3 }).Value,
            ImageContentType.JPEG
        );

        ApplicationDbContext.Products.Add(product);
        ApplicationDbContext.Images.Add(image);
        await ApplicationDbContext.SaveChangesAsync();

        product.AddImage(image);
        await ApplicationDbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{productId}/images/{image.Id.Value}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify image was removed from category
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedProduct = await ApplicationDbContext.Products
            .Include(c => c.Images)
            .FirstAsync(c => c.Id == product.Id);
        updatedProduct.Images.Should().NotContain(i => i.Id == image.Id);
    }

    [Fact]
    public async Task WhenImageNotFound_ThenReturnsNotFound()
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
        product.StockQuantity = StockQuantity.Create(1).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync();

        var nonExistentImageId = Guid.NewGuid();

        var expectedJson =
            MakeSystemErrorApiOutput(
                new DeleteProductImageCommandHandler.ImageNotFoundError(nonExistentImageId, productId));

        var request =
            new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{productId}/images/{nonExistentImageId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenProductNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        var expectedJson =
            MakeSystemErrorApiOutput(new DeleteProductImageCommandHandler.ProductNotFoundError(productId));

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{productId}/images/{imageId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var request =
            new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{Guid.NewGuid()}/images/{Guid.NewGuid()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenNotSeller_ThenReturnsUnauthorized()
    {
        // Arrange
        var request =
            new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{Guid.NewGuid()}/images/{Guid.NewGuid()}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Default"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}