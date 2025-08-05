using System.Text.Json;
using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Commands.CreateProduct;
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

namespace Catalog.Tests.Integration.Application.UseCases.Products.Commands;

public class CreateProductCommandHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;

    public CreateProductCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _cache = Substitute.For<IDistributedCache>();
    }

    private Category CreateTestCategory(Guid categoryId) => Category.Create(
        CategoryId.Create(categoryId).Value,
        CategoryName.Create("SampleCategory").Value,
        CategoryDescription.Create("Sample description").Value
    );

    private ProductWriteDto CreateTestProductWriteDto(Guid id, Guid categoryId, Guid? sellerId = null) => new ProductWriteDto(
        Id: id,
        Title: "New Product",
        Description: "New Description",
        Price: 20,
        CategoryId: categoryId,
        SellerId: sellerId ?? Guid.NewGuid(),
        Quantity: 100
    );

    [Fact]
    public async Task WhenProductAlreadyExists_ThenReturnsFailureResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        var existingProduct = Product.Create(
            ProductId.Create(id).Value,
            ProductTitle.Create("Existing Product").Value,
            ProductDescription.Create("Existing Description").Value,
            ProductPrice.Create(10).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            CategoryId.Create(categoryId).Value
        );
        existingProduct.StockQuantity = StockQuantity.Create(100).Value;

        ApplicationDbContext.Categories.Add(category);
        ApplicationDbContext.Products.Add(existingProduct);
        await ApplicationDbContext.SaveChangesAsync(default);

        var createProductDto = CreateTestProductWriteDto(id, categoryId);

        var cmd = new CreateProductCommand(createProductDto);
        var handler = new CreateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is CreateProductCommandHandler.ProductExistsError);
    }

    [Fact]
    public async Task WhenCategoryNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var nonExistentCategoryId = Guid.NewGuid();

        var createProductDto =  CreateTestProductWriteDto(id, nonExistentCategoryId);

        var cmd = new CreateProductCommand(createProductDto);
        var handler = new CreateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is CreateProductCommandHandler.CategoryNotFoundError);
    }

    [Fact]
    public async Task WhenValidData_ThenCreatesProduct_SavesContext_AndCachesDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);

        ApplicationDbContext.Categories.Add(category);
        await ApplicationDbContext.SaveChangesAsync(default);

        var createProductDto = CreateTestProductWriteDto(id, categoryId);
        
        ConfigureMapster();
        var cmd = new CreateProductCommand(createProductDto);
        var handler = new CreateProductCommandHandler(ApplicationDbContext, _cache);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify product created in DB
        var createdProduct = await ApplicationDbContext.Products
            .Include(p => p.Category)
            .ThenInclude(c => c!.Images)
            .FirstOrDefaultAsync(p => p.Id == ProductId.Create(id).Value);

        createdProduct.Should().NotBeNull();
        createdProduct.Title.Value.Should().Be(createProductDto.Title);
        createdProduct.Description.Value.Should().Be(createProductDto.Description);
        createdProduct.Price.Value.Should().Be(createProductDto.Price);
        createdProduct.CategoryId!.Value.Should().Be(createProductDto.CategoryId);
        createdProduct.StockQuantity.Value.Should().Be(createProductDto.Quantity);

        // Verify cache update
        var expectedDto = createdProduct.Adapt<ProductReadDto>();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(expectedDto);

        await _cache.Received(1).SetAsync(
            $"product-{id}",
            Arg.Is<byte[]>(b => b.SequenceEqual(bytes)),
            Arg.Any<DistributedCacheEntryOptions>(),
            Arg.Any<CancellationToken>()
        );
    }
}