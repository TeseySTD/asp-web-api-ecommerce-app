using Catalog.Application.UseCases.Categories.Commands.DeleteCategory;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.Commands;

public class DeleteCategoryCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public DeleteCategoryCommandHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var nonExistentCategoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(CategoryId.Create(nonExistentCategoryId).Value);
        var handler = new DeleteCategoryCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e == Error.NotFound);
    }

    [Fact]
    public async Task WhenCategoryExists_NoProducts_ThenDeletesCategory_AndRemovesCategoryCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("CategoryToDelete").Value,
            CategoryDescription.Create("To be deleted").Value
        );
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var command = new DeleteCategoryCommand(CategoryId.Create(categoryId).Value);
        var handler = new DeleteCategoryCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exists = await ApplicationDbContext.Categories.AnyAsync(c => c.Id == CategoryId.Create(categoryId).Value);
        exists.Should().BeFalse();
        await _cache.Received(1).RemoveAsync($"category-{categoryId}");
    }

    [Fact]
    public async Task WhenCategoryExists_WithProducts_ThenDeletesCategory_AndRemovesProductAndCategoryCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("CategoryWithProducts").Value,
            CategoryDescription.Create("Has products").Value
        );
        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var productIds = new List<Guid>();
        for (int i = 0; i < 3; i++)
        {
            var prodId = Guid.NewGuid();
            productIds.Add(prodId);
            var product = Product.Create(
                ProductId.Create(prodId).Value,
                ProductTitle.Create($"Title {i}").Value,
                ProductDescription.Create("Desccription").Value,
                ProductPrice.Create(1m).Value,
                category.Id
            );
            product.StockQuantity = StockQuantity.Create(1).Value;
            ApplicationDbContext.Products.Add(product);
        }

        await ApplicationDbContext.SaveChangesAsync(default);

        var command = new DeleteCategoryCommand(CategoryId.Create(categoryId).Value);
        var handler = new DeleteCategoryCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var exists = await ApplicationDbContext.Categories.AnyAsync(c => c.Id == CategoryId.Create(categoryId).Value);
        exists.Should().BeFalse();
        
        foreach (var prodId in productIds)
        {
            await _cache.Received(1).RemoveAsync($"product-{prodId}");
        }

        await _cache.Received(1).RemoveAsync($"category-{categoryId}");
    }
}