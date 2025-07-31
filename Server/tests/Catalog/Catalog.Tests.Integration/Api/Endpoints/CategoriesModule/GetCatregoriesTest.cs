using System.Net;
using Catalog.API.Http.Category.Responses;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Newtonsoft.Json;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Api.Endpoints.CategoriesModule;

public class GetCatregoriesTest : ApiTest
{
    public GetCatregoriesTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture,
        CacheFixture cacheFixture) : base(factory, databaseFixture, cacheFixture)
    {
    }

    private const string RequestUrl = "/api/categories";


    [Fact]
    public async Task WhenNoCategoriesExist_ThenReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?page=1&pageSize=10");
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        Assert.Equal(MakeSystemErrorApiOutput(Error.NotFound), json, ignoreAllWhiteSpace: true);
    }


    [Fact]
    public async Task WhenCategoriesExist_ReturnsOk()
    {
        // Arrange
        var category1 = Category.Create(
            CategoryId.Create(Guid.NewGuid()).Value,
            CategoryName.Create("Electronics").Value,
            CategoryDescription.Create("Electronic devices").Value
        );

        var category2 = Category.Create(
            CategoryId.Create(Guid.NewGuid()).Value,
            CategoryName.Create("Books").Value,
            CategoryDescription.Create("All kinds of books").Value
        );
        Category[] categories = [category1, category2];

        ApplicationDbContext.Categories.AddRange(categories);
        await ApplicationDbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?page=0&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<GetCategoriesResponse>(content);

        result.Should().NotBeNull();
        result.Categories.Data.Should().HaveCount(2);
        foreach (var c in categories)
        {
            result.Categories.Data.Should().Contain(d => 
                d.Id == c.Id.Value &&
                d.Name == c.Name.Value &&
                d.Description == c.Description.Value 
            );
        }
    }
}