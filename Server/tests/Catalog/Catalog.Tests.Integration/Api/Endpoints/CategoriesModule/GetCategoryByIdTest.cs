using System.Net;
using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Newtonsoft.Json;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Api.Endpoints.CategoriesModule;

public class GetCategoryByIdTest : ApiTest
{
    public GetCategoryByIdTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/categories";

    [Fact]
    public async Task WhenNoCategoriesExist_ThenReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{id}");
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(MakeSystemErrorApiOutput(Error.NotFound), json, ignoreAllWhiteSpace: true);
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

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{categoryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<CategoryReadDto>(content);

        result.Should().NotBeNull();
        result.Id.Should().Be(categoryId);
        result.Name.Should().Be("Test Category");
        result.Description.Should().Be("Test Description");
    }
}