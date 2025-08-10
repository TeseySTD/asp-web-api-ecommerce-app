using System.Net;
using System.Net.Http.Headers;
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

    private HttpRequestMessage GenereateHttpRequest(UpdateCategoryRequest dto, Guid categoryId, string role = "Admin")
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{categoryId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var json = JsonConvert.SerializeObject(dto);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return request;
    }

    [Fact]
    public async Task UpdateCategory_Unauthorized_ReturnsUnauthorized()
    {
        // Act
        var response = await HttpClient.PutAsync( $"{RequestUrl}/{Guid.NewGuid()}", new StringContent("", Encoding.UTF8, "application/json"));

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_CustomerIsNotAdmin_ReturnsForbidden()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var dto = new UpdateCategoryRequest("New Category", "New Description");
        var request = GenereateHttpRequest(dto, categoryId, "Seller");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCategory_ValidData_ReturnsOk()
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

        var dto = new UpdateCategoryRequest("Updated Name", "Updated Description");
        var request = GenereateHttpRequest(dto, categoryId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify category was updated
        ApplicationDbContext.ChangeTracker.Clear();
        var updatedCategory = await ApplicationDbContext.Categories.FirstAsync(c => c.Id == category.Id);
        updatedCategory.Name.Value.Should().Be("Updated Name");
        updatedCategory.Description.Value.Should().Be("Updated Description");
    }

    [Fact]
    public async Task UpdateCategory_CategoryNotInDb_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var dto = new UpdateCategoryRequest("Updated Name", "Updated Description");

        var expectedJson = MakeSystemErrorApiOutput(Error.NotFound);

        var request = GenereateHttpRequest(dto, nonExistentId);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}