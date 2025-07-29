using System.Text.Json;
using Catalog.Application.Dto.Category;
using Catalog.Application.UseCases.Categories.Commands.DeleteCategoryImage;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.Commands;

public class DeleteCategoryImageCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public DeleteCategoryImageCommandHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsCategoryNotFoundError()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        var command = new DeleteCategoryImageCommand(nonExistentCategoryId, Guid.NewGuid());
        var handler = new DeleteCategoryImageCommandHandler(_cache, ApplicationDbContext);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteCategoryImageCommandHandler.CategoryNotFoundError);
    }

    [Fact]
    public async Task WhenImageNotFoundOrNotBelongToCategory_ThenReturnsImageNotFoundError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Category").Value,
            CategoryDescription.Create("Description").Value
        );
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var nonExistentImageId = Guid.NewGuid();
        var command = new DeleteCategoryImageCommand(categoryId, nonExistentImageId);
        var handler = new DeleteCategoryImageCommandHandler(_cache, ApplicationDbContext);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteCategoryImageCommandHandler.ImageNotFoundError);
    }

    [Fact]
    public async Task WhenValidImage_ThenRemovesImage_SavesContext_AndUpdatesCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Category").Value,
            CategoryDescription.Create("Description").Value
        );
        var imageId = Guid.NewGuid();
        var image = Image.Create(
            ImageId.Create(imageId).Value,
            FileName.Create("img.jpg").Value,
            ImageData.Create(new byte[] { 1, 2 }).Value,
            ImageContentType.JPEG
        );
        ApplicationDbContext.Images.Add(image);
        category.AddImage(image);
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var command = new DeleteCategoryImageCommand(categoryId, imageId);
        ConfigureMapster();
        var handler = new DeleteCategoryImageCommandHandler(_cache, ApplicationDbContext);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updatedCategory = await ApplicationDbContext.Categories
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == CategoryId.Create(categoryId).Value);
        updatedCategory!.Images.Should().BeEmpty();

        var bytes = JsonSerializer.SerializeToUtf8Bytes(updatedCategory.Adapt<CategoryReadDto>());
        await _cache.Received(1).SetAsync(
            $"category-{categoryId}",
            Arg.Is<byte[]>(b => b.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>()
        );
    }
}