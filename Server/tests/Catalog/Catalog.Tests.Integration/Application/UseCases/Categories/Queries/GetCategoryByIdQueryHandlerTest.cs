using System.Text.Json;
using Catalog.Application.Dto.Category;
using Catalog.Application.UseCases.Categories.Queries.GetCategoryById;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.Queries;

public class GetCategoryByIdQueryHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;
    private readonly GetCategoryByIdQueryHandler _handler;

    public GetCategoryByIdQueryHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
        _handler = new GetCategoryByIdQueryHandler(ApplicationDbContext, _cache);
        ConfigureMapster();
    }

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("Test").Value,
        CategoryDescription.Create("Test description").Value
    );

    [Fact]
    public async Task WhenCached_ReturnsFromCacheWithoutDbCall()
    {
        // Arrange
        var existingCategory = CreateTestCategory(Guid.NewGuid()); 
        ApplicationDbContext.Categories.Add(existingCategory);
        await ApplicationDbContext.SaveChangesAsync(default);

        var categoryIdValue = existingCategory.Id.Value;

        var dto = new CategoryReadDto(categoryIdValue, "CachedName", "CachedDescription", Array.Empty<string>());
        var serializedDto = JsonSerializer.SerializeToUtf8Bytes(dto);
        _cache.GetAsync($"category-{categoryIdValue}").Returns(serializedDto);

        // Act
        var result = await _handler.Handle(new GetCategoryByIdQuery(CategoryId.Create(categoryIdValue).Value), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(dto);
        // No cache set when exists in cache
        await _cache.DidNotReceive()
            .SetAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<DistributedCacheEntryOptions>());
    }

    [Fact]
    public async Task WhenNotCachedAndExists_CachesAndReturnsCategory()
    {
        // Arrange
        var category = CreateTestCategory(Guid.NewGuid());
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        _cache.GetAsync($"category-{category.Id.Value}").Returns([]);

        // Act
        var result = await _handler.Handle(new GetCategoryByIdQuery(category.Id), default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(category.Id.Value);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(category.Adapt<CategoryReadDto>());
        await _cache.Received(1).SetAsync(
            $"category-{category.Id.Value}",
            Arg.Is<byte[]>(b => b.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>());
    }

    [Fact]
    public async Task WhenNotCachedAndNotExists_ReturnsNotFoundError()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        _cache.GetAsync($"category-{nonExistentId}").Returns([]);

        var result = await _handler.Handle(new GetCategoryByIdQuery(CategoryId.Create(nonExistentId).Value), default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e == Error.NotFound);
    }
}