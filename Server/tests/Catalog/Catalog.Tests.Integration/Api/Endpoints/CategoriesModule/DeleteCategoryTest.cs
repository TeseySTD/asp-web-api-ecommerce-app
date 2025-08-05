using System.Net;
using System.Net.Http.Headers;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Integration.Api.Endpoints.CategoriesModule;

public class DeleteCategoryTest : ApiTest
{
    public DeleteCategoryTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/categories";

    private HttpRequestMessage GenereateHttpRequest(Guid categoryId, string role = "Admin")
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{categoryId}");
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
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerIsNotAdmin_ThenReturnsForbidden()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var request = GenereateHttpRequest(categoryId, "Seller");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WhenCategoryExists_ThenReturnsOk()
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

        var request = GenereateHttpRequest(categoryId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify category was deleted
        ApplicationDbContext.ChangeTracker.Clear();
        var deletedCategory = await ApplicationDbContext.Categories
            .FirstOrDefaultAsync(c => c.Id == category.Id);
        deletedCategory.Should().BeNull();
    }

    [Fact]
    public async Task WhenCategoryNotFound_ReturnsBadRequest()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        var request = GenereateHttpRequest(nonExistentId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}