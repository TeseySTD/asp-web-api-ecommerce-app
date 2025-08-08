using Catalog.Application.UseCases.Categories.Commands.DeleteCategory;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shared.Core.Validation.Result;
using Shared.Messaging.Events.Category;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.Commands;

public class DeleteCategoryCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;

    public DeleteCategoryCommandHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
    }

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("Test category").Value,
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
        var nonExistentCategoryId = Guid.NewGuid();
        var command = new DeleteCategoryCommand(CategoryId.Create(nonExistentCategoryId).Value);
        var handler = new DeleteCategoryCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e == Error.NotFound);
    }

    [Fact]
    public async Task WhenCategoryExists_WithoutProducts_ThenDeletesCategoryAndRemovesCategoryCacheOnly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);
        ApplicationDbContext.Categories.Add(category);

        var products = CreateTestProducts();
        ApplicationDbContext.Products.AddRange(products);
        await ApplicationDbContext.SaveChangesAsync(default);

        var command = new DeleteCategoryCommand(CategoryId.Create(categoryId).Value);
        var handler = new DeleteCategoryCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var exists = await ApplicationDbContext.Categories.AnyAsync(c => c.Id == CategoryId.Create(categoryId).Value);
        exists.Should().BeFalse();
        await _cache.Received(1).RemoveAsync($"category-{categoryId}");

        // Check that only category cache was cleared
        foreach (var p in products)
        {
            await _cache.DidNotReceive().RemoveAsync($"product-{p.Id.Value}");
        }

        await _publishEndpoint.Received(1).Publish(Arg.Is<CategoryDeletedEvent>(
            e => e.CategoryId == categoryId
        ));
    }

    [Fact]
    public async Task WhenCategoryExistsWithProducts_ThenDeletesCategoryAndRemovesProductAndCategoryCache()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);
        ApplicationDbContext.Categories.Add(category);

        var products = CreateTestProducts(category.Id);

        ApplicationDbContext.Products.AddRange(products);
        await ApplicationDbContext.SaveChangesAsync(default);

        var command = new DeleteCategoryCommand(CategoryId.Create(categoryId).Value);
        var handler = new DeleteCategoryCommandHandler(ApplicationDbContext, _cache, _publishEndpoint);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var exists = await ApplicationDbContext.Categories.AnyAsync(c => c.Id == CategoryId.Create(categoryId).Value);
        exists.Should().BeFalse();

        foreach (var p in products)
        {
            await _cache.Received(1).RemoveAsync($"product-{p.Id.Value}");
        }

        await _cache.Received(1).RemoveAsync($"category-{categoryId}");
        
        await _publishEndpoint.Received(1).Publish(Arg.Is<CategoryDeletedEvent>(
            e => e.CategoryId == categoryId
        ));
    }
}