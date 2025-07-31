using System.Net;
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

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{categoryId}");

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

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}