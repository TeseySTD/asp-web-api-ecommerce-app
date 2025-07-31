using System.Net;
using System.Text;
using Catalog.API.Http.Category.Requests;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Catalog.Tests.Integration.Api.Endpoints.CategoriesModule;

public class CreateCategoryTest : ApiTest
{
    public CreateCategoryTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/categories";

    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var request = new AddCategoryRequest("New Category", "New Description");
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify category was created in database
        var categories = await ApplicationDbContext.Categories.ToListAsync();
        categories.Should().ContainSingle(c => c.Name.Value == "New Category");
    }

    [Fact]
    public async Task WhenInvalidData_ThenReturnsBadRequest()
    {
        // Arrange
        var request = new AddCategoryRequest("", "Description"); // Invalid empty name
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var expectedJson = MakePropertyErrorApiOutput("Name", [new CategoryName.NameRequiredError()]);

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}