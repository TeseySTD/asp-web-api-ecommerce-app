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

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("Test").Value,
        CategoryDescription.Create("Test description").Value
    );

    private List<Product> CreateTestProducts(CategoryId? categoryId = null)
    {
        var products = new List<Product>();
        for (int i = 0; i < 3; i++)
        {
            var product = Product.Create(
                ProductId.Create(Guid.NewGuid()).Value,
                ProductTitle.Create($"Title {i}").Value,
                ProductDescription.Create("Desccription").Value,
                ProductPrice.Create(1m).Value,
                SellerId.Create(Guid.NewGuid()).Value,
                categoryId
            );
            product.StockQuantity = StockQuantity.Create(1).Value;
            products.Add(product);
        }

        return products;
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
        var category = CreateTestCategory(categoryId);
        ApplicationDbContext.Categories.Add(category);

        var products = CreateTestProducts(category.Id);
        ApplicationDbContext.Products.AddRange(products);
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
        foreach (var p in products)
        {
            await _cache.Received(1).RemoveAsync($"product-{p.Id.Value}");
        }

    }

    [Fact]
    public async Task WhenCategoryHasNoProducts_ThenUpdatesCategoryWithoutProductCacheInvalidation()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);
        ApplicationDbContext.Categories.Add(category);
        
        var products = CreateTestProducts();
        ApplicationDbContext.Products.AddRange(products);
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

        // Check that only category cache was cleared
        foreach (var p in products)
        {
            await _cache.DidNotReceive().RemoveAsync($"product-{p.Id.Value}");
        }
    }
}