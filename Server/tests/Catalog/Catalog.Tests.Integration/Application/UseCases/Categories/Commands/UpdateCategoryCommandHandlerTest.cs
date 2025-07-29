using System.Text.Json;
using Catalog.Application.Dto.Category;
using Catalog.Application.UseCases.Categories.Commands.UpdateCategory;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.Commands;

public class UpdateCategoryCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public UpdateCategoryCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateDto = new CategoryWriteDto(
            Id: nonExistentId,
            Name: "Updated Category",
            Description: "Updated Description"
        );
        
        var cmd = new UpdateCategoryCommand(updateDto);
        var handler = new UpdateCategoryCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == Error.NotFound);
    }

    [Fact]
    public async Task WhenCategoryHasProducts_ThenUpdatesCategoryAndInvalidatesProductCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Electronics").Value,
            CategoryDescription.Create("Electronic devices").Value
        );

        var product1Id = Guid.NewGuid();
        var product1 = Product.Create(
            ProductId.Create(product1Id).Value,
            ProductTitle.Create("Laptop").Value,
            ProductDescription.Create("Gaming laptop").Value,
            ProductPrice.Create(1500).Value,
            CategoryId.Create(categoryId).Value
        );
        product1.StockQuantity = StockQuantity.Create(1).Value;

        var product2Id = Guid.NewGuid();
        var product2 = Product.Create(
            ProductId.Create(product2Id).Value,
            ProductTitle.Create("Phone").Value,
            ProductDescription.Create("Smart phone").Value,
            ProductPrice.Create(800).Value,
            CategoryId.Create(categoryId).Value
        );
        product2.StockQuantity = StockQuantity.Create(1).Value;
        
        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Products.AddRange(product1, product2);
        await ApplicationDbContext.SaveChangesAsync(default);

        var updateDto = new CategoryWriteDto(
            Id: categoryId,
            Name: "Updated Electronics",
            Description: "Updated electronic devices"
        );

        ConfigureMapster();
        var cmd = new UpdateCategoryCommand(updateDto);
        var handler = new UpdateCategoryCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify category updated
        var updatedCategory = await ApplicationDbContext.Categories
            .FirstAsync(c => c.Id == category.Id);
        
        updatedCategory.Name.Value.Should().Be(updateDto.Name);
        updatedCategory.Description.Value.Should().Be(updateDto.Description);

        // Verify category cache update
        await _cache.Received(1).SetAsync(
            $"category-{categoryId}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        );

        // Verify product cache invalidation
        await _cache.Received(1).RemoveAsync($"product-{product1Id}");
        await _cache.Received(1).RemoveAsync($"product-{product2Id}");
    }

    [Fact]
    public async Task WhenCategoryHasNoProducts_ThenUpdatesCategoryWithoutProductCacheInvalidation()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Empty Category").Value,
            CategoryDescription.Create("Category with no products").Value
        );
        
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var updateDto = new CategoryWriteDto(
            Id: categoryId,
            Name: "Updated Empty Category",
            Description: "Still no products here"
        );

        ConfigureMapster();
        var cmd = new UpdateCategoryCommand(updateDto);
        var handler = new UpdateCategoryCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify category updated
        var updatedCategory = await ApplicationDbContext.Categories
            .FirstAsync(c => c.Id == category.Id);
        
        updatedCategory.Name.Value.Should().Be(updateDto.Name);
        updatedCategory.Description.Value.Should().Be(updateDto.Description);

        // Verify category cache update
        await _cache.Received(1).SetAsync(
            $"category-{categoryId}",
            Arg.Any<byte[]>(),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        );

        // Verify no product cache invalidation calls (since no products exist)
        var removeCalls = _cache.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == "RemoveAsync")
            .ToList();
        
        removeCalls.Should().BeEmpty();
    }
}
