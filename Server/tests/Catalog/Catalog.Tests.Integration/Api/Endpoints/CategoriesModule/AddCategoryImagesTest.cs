using System.Net;
using System.Net.Http.Headers;
using Catalog.API.Endpoints;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Integration.Api.Endpoints.CategoriesModule;

public class AddCategoryImagesTest : ApiTest
{
    public AddCategoryImagesTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/categories";

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("Test Category").Value,
        CategoryDescription.Create("Test Description").Value
    );

    private HttpRequestMessage GenereateHttpRequest(Guid categoryId, MultipartFormDataContent form,
        string role = "Admin")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}/{categoryId}/images");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = form;
        return request;
    }

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        using var form = new MultipartFormDataContent();
        var imageContent1 = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        imageContent1.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        form.Add(imageContent1, "images", "test1.jpg");
        
        // Act
        var response = await HttpClient.PostAsync($"{RequestUrl}/{categoryId}/images", form);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerIsNotAdmin_ThenReturnsForbidden()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        using var form = new MultipartFormDataContent();
        var imageContent1 = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        imageContent1.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        form.Add(imageContent1, "images", "test1.jpg");
        
        var request = GenereateHttpRequest(categoryId, form, "Seller");
        
        // Act
        var response = await HttpClient.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    

    [Fact]
    public async Task WhenValidImages_ThenReturnsOk()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();

        var imageContent1 = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        imageContent1.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        form.Add(imageContent1, "images", "test1.jpg");

        var imageContent2 = new ByteArrayContent(new byte[] { 5, 6, 7, 8 });
        imageContent2.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
        form.Add(imageContent2, "images", "test2.png");

        var request = GenereateHttpRequest(categoryId, form);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify images were added
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedCategory = await ApplicationDbContext.Categories
            .Include(c => c.Images)
            .FirstAsync(c => c.Id == category.Id);
        updatedCategory.Images.Should().HaveCount(2);
    }

    [Fact]
    public async Task WhenNoImages_ThenReturnsBadRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();

        var request = GenereateHttpRequest(categoryId, form);

        // Act
        var response = await HttpClient.SendAsync(request);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    [Fact]
    public async Task WhenImagesAreExceededMaxCount_ThenReturnsBadRequest()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();

        for (int i = 0; i < Category.MaxImagesCount + 1; i++)
        {
            var img = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
            img.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
            form.Add(img, "images", $"test{i}.jpg");
        }

        var expectedJson = MakeSystemErrorApiOutput(new CategoryModule.ImageCountOutOfRangeError());
        var request = GenereateHttpRequest(categoryId, form);

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
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        using var form = new MultipartFormDataContent();
        var textContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        textContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
        form.Add(textContent, "images", "test.txt");

        var expectedJson = MakeSystemErrorApiOutput(new CategoryModule.InvalidImagesTypeError());

        var request = GenereateHttpRequest(categoryId, form);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        using var form = new MultipartFormDataContent();

        var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3, 4 });
        imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
        form.Add(imageContent, "images", "test1.jpg");

        var request = GenereateHttpRequest(categoryId, form);

        // Act
        var response = await HttpClient.SendAsync(request);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}