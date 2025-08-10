using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Application.UseCases.Categories.Commands.CreateCategory;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.Commands;

public class CreateCategoryCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public CreateCategoryCommandHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreateCategoryAndCachesDto()
    {
        // Arrange
        var categoryName = "NewCategory";
        var categoryDescription = "New category description";

        var command = new CreateCategoryCommand(categoryName, categoryDescription);
        ConfigureMapster();
        var handler = new CreateCategoryCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var createdCategoryId = result.Value;
        var createdCategory =
            await ApplicationDbContext.Categories.FindAsync(CategoryId.Create(createdCategoryId.Value).Value);
        createdCategory.Should().NotBeNull();
        createdCategory.Name.Value.Should().Be(categoryName);
        createdCategory.Description.Value.Should().Be(categoryDescription);

        // Verify cache set
        var bytes = JsonSerializer.SerializeToUtf8Bytes(createdCategory.Adapt<CategoryReadDto>());
        await _cache.Received(1).SetAsync(
            $"category-{createdCategoryId.Value}",
            Arg.Is<byte[]>(s => s.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>());
    }

    [Fact]
    public async Task Handle_SaveMethodThrowsException_ReturnsFailureResult()
    {
        // Arrange
        var categoryName = "FaultyCategory";
        var categoryDescription = "Cause exception";
        
        // Replace context to throw on save
        var faultyContext = Substitute.For<IApplicationDbContext>();
        var exception = new InvalidOperationException("Database error");
        faultyContext.Categories.Returns(ApplicationDbContext.Categories);
        faultyContext.When(c => c.SaveChangesAsync(Arg.Any<CancellationToken>()))
            .Do(_ => throw exception );

        var command = new CreateCategoryCommand(categoryName, categoryDescription);
        var handler = new CreateCategoryCommandHandler(faultyContext, _cache);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains(exception.Message));
    }
}