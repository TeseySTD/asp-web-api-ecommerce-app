using System.Net;
using System.Text;
using Catalog.API.Http.Category.Requests;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Api.Endpoints.CategoriesModule;

public class UpdateCategoryTest : ApiTest
{
    public UpdateCategoryTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/categories";

    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Original Name").Value,
            CategoryDescription.Create("Original Description").Value
        );

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync();

        var request = new UpdateCategoryRequest("Updated Name", "Updated Description");
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{categoryId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify category was updated
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedCategory = await ApplicationDbContext.Categories.FirstAsync(c => c.Id == category.Id);
        updatedCategory.Name.Value.Should().Be("Updated Name");
        updatedCategory.Description.Value.Should().Be("Updated Description");
    }

    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var request = new UpdateCategoryRequest("Updated Name", "Updated Description");
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var expectedJson = MakeSystemErrorApiOutput(Error.NotFound);
        
        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{nonExistentId}", content);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}