using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Catalog.API.Http.Product.Requests;
using Catalog.Application.UseCases.Products.Commands.CreateProduct;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Catalog.Tests.Integration.Api.Endpoints.ProductsModule;

public class CreateProductTest : ApiTest
{
    public CreateProductTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/products";

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var request = new AddProductRequest
        (
            "Test Product",
            "Test Description",
            99.99m,
            10,
            Guid.NewGuid()
        );
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync($"{RequestUrl}/", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenNotSeller_ThenReturnsForbidden()
    {
        // Arrange
        var dto = new AddProductRequest
        (
            "Test Product",
            "Test Description",
            99.99m,
            10,
            Guid.NewGuid()
        );
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Default"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task WhenRequsetIsValid_ThenReturnsOk()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Test Category").Value,
            CategoryDescription.Create("Test Description").Value
        );
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        var dto = new AddProductRequest
        (
            "Test Product",
            "Test Description",
            99.99m,
            10,
            categoryId
        );

        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Check if products in db
        var products = await ApplicationDbContext.Products.ToListAsync();
        products.Should().ContainSingle(p => p.Title.Value == dto.Title);
    }

    [Fact]
    public async Task WhenInvalidData_ThenReturnsBadRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Test Category").Value,
            CategoryDescription.Create("Test Description").Value
        );
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        var dto = new AddProductRequest
        (
            "", // Empty title 
            "Test Description",
            99.99m,
            10,
            categoryId
        );

        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        var expectedJson = MakePropertyErrorApiOutput("Title", [new ProductTitle.TitleRequiredError(), new ProductTitle.OutOfLengthError()]);

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
    
    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsBadRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        var dto = new AddProductRequest
        (
            "Test Title", 
            "Test Description",
            99.99m,
            10,
            categoryId
        );

        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Seller"
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        var expectedJson = MakeSystemErrorApiOutput(new CreateProductCommandHandler.CategoryNotFoundError(categoryId)); 

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}