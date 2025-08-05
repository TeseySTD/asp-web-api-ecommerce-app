using System.Net;
using System.Net.Http.Headers;
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

    private HttpRequestMessage GenereateHttpRequest(AddCategoryRequest dto, string role = "Admin")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = role
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var json = JsonConvert.SerializeObject(dto);
        request.Content =  new StringContent(json, Encoding.UTF8, "application/json");
        return request;
    }
 
    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Act
        var response = await HttpClient.PostAsync(RequestUrl, new StringContent("", Encoding.UTF8, "application/json"));
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerIsNotAdmin_ThenReturnsForbidden()
    {
        // Arrange
        var dto = new AddCategoryRequest("New Category", "New Description");
        var request = GenereateHttpRequest(dto, "Seller");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    [Fact]
    public async Task WhenValidData_ThenReturnsOk()
    {
        // Arrange
        var dto = new AddCategoryRequest("New Category", "New Description");
        var request = GenereateHttpRequest(dto);

        // Act
        var response = await HttpClient.SendAsync(request);

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
        var dto = new AddCategoryRequest("", "Description"); // Invalid empty name
        var request = GenereateHttpRequest(dto);

        var expectedJson = MakePropertyErrorApiOutput("Name", [new CategoryName.NameRequiredError()]);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }
}