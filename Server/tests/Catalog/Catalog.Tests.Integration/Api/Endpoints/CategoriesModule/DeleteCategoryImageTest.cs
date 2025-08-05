using System.Net;
using System.Net.Http.Headers;
using Catalog.Application.UseCases.Categories.Commands.DeleteCategoryImage;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Integration.Api.Endpoints.CategoriesModule;

public class DeleteCategoryImageTest : ApiTest
{
    public DeleteCategoryImageTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/categories";

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("Test Category").Value,
        CategoryDescription.Create("Test Description").Value
    );

    private HttpRequestMessage GenereateHttpRequest(Guid categoryId, Guid imageId, string role = "Admin")
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{categoryId}/images/{imageId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{Guid.NewGuid()}/images/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerIsNotAdmin_ThenReturnsForbidden()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();
        var request = GenereateHttpRequest(categoryId, imageId, "Seller");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        var image = Image.Create(
            FileName.Create("test.jpg").Value,
            ImageData.Create(new byte[] { 1, 2, 3 }).Value,
            ImageContentType.JPEG
        );

        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Images.Add(image);
        await ApplicationDbContext.SaveChangesAsync();

        category.AddImage(image);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenereateHttpRequest(categoryId, image.Id.Value);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify image was removed from category
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedCategory = await ApplicationDbContext.Categories
            .Include(c => c.Images)
            .FirstAsync(c => c.Id == category.Id);
        updatedCategory.Images.Should().NotContain(i => i.Id == image.Id);
    }

    [Fact]
    public async Task WhenImageNotFound_ThneReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        var nonExistentImageId = Guid.NewGuid();

        var expectedJson =
            MakeSystemErrorApiOutput(
                new DeleteCategoryImageCommandHandler.ImageNotFoundError(nonExistentImageId, categoryId));

        var request = GenereateHttpRequest(categoryId, nonExistentImageId);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }


    [Fact]
    public async Task WhenCategoryNotFound_ThneReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var imageId = Guid.NewGuid();

        var expectedJson =
            MakeSystemErrorApiOutput(new DeleteCategoryImageCommandHandler.CategoryNotFoundError(categoryId));

        var request = GenereateHttpRequest(categoryId, imageId);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}