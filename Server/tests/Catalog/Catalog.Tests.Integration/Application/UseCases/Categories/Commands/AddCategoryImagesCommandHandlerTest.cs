using System.Text.Json;
using Catalog.Application.Dto.Category;
using Catalog.Application.Dto.Image;
using Catalog.Application.UseCases.Categories.Commands.AddCategoryImages;
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

public class AddCategoryImagesCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public AddCategoryImagesCommandHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("Category").Value,
        CategoryDescription.Create("Description").Value
    );

    [Fact]
    public async Task Handle_CategoryNotInDb_ReturnsCategoryNotFoundError()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        var images = new[] { new ImageDto("file1.jpg", "Jpeg", new byte[0]) };
        var cmd = new AddCategoryImagesCommand(nonExistentCategoryId, images);
        var handler = new AddCategoryImagesCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is AddCategoryImagesCommandHandler.CategoryNotFoundError);
    }

    [Fact]
    public async Task Handle_ImagesExceedMaximum_ReturnsImagesOutOfRangeError()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);
        for (int i = 0; i < Category.MaxImagesCount; i++)
        {
            var img = Image.Create(FileName.Create($"f{i}.jpg").Value, ImageData.Create(new byte[0]).Value,
                ImageContentType.JPEG);
            category.AddImage(img);
            ApplicationDbContext.Images.Add(img);
        }

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Prepare two images to exceed max
        var images = Enumerable.Range(0, 2)
            .Select(i => new ImageDto($"file{i}.jpg", "Jpeg", new byte[0]))
            .ToArray();
        var cmd = new AddCategoryImagesCommand(categoryId, images);
        var handler = new AddCategoryImagesCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is AddCategoryImagesCommandHandler.ImagesOutOfRangeError);
    }

    [Fact]
    public async Task Handle_ValidImages_ShouldAddImagesAndCachesDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId); 
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var images = new[]
        {
            new ImageDto("a.jpg", "Jpeg", new byte[] { 1, 2, 3 }),
            new ImageDto("b.png", "Png", new byte[] { 4, 5, 6 })
        };
        var cmd = new AddCategoryImagesCommand(categoryId, images);
        
        ConfigureMapster();
        var handler = new AddCategoryImagesCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var updated = await ApplicationDbContext.Categories
            .Include(c => c.Images)
            .FirstOrDefaultAsync(c => c.Id == CategoryId.Create(categoryId).Value);
        updated.Should().NotBeNull();
        updated.Images.Count.Should().Be(2);

        var bytes = JsonSerializer.SerializeToUtf8Bytes(updated.Adapt<CategoryReadDto>());
        await _cache.Received(1).SetAsync(
            $"category-{categoryId}",
            Arg.Is<byte[]>(s => s.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>());
    }
}