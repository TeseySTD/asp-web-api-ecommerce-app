using Catalog.Application.UseCases.Categories.Queries.GetCategories;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Shared.Core.API;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.Queries;

public class GetCategoriesQueryHandlerTest : IntegrationTest
{
    private readonly GetCategoriesQueryHandler _handler;

    public GetCategoriesQueryHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _handler = new GetCategoriesQueryHandler(ApplicationDbContext);
        ConfigureMapster();
    }

    [Fact]
    public async Task WhenNoCategories_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetCategoriesQuery(new PaginationRequest());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e == Error.NotFound);
    }

    [Fact]
    public async Task WhenCategoriesExist_ReturnsPaginatedResult()
    {
        // Arrange
        var pageIndex = 1;
        var pageSize = 2;
        var query = new GetCategoriesQuery(new PaginationRequest(pageIndex, pageSize));
        
        for (var i = 1; i <= 5; i++)
        {
            var category = Category.Create(
                CategoryId.Create(Guid.NewGuid()).Value,
                CategoryName.Create($"Category{i}").Value,
                CategoryDescription.Create($"Description{i}").Value
            );
            ApplicationDbContext.Categories.Add(category);
        }

        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var page = result.Value;
        page.PageIndex.Should().Be(pageIndex);
        page.PageSize.Should().Be(pageSize);
        page.Data.Count().Should().Be(pageSize);
        page.Data.All(c => c != null).Should().BeTrue();
    }
}
