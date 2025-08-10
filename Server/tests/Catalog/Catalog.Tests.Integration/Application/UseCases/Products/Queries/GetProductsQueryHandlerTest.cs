using Catalog.Application.Dto.Product;
using Catalog.Application.UseCases.Products.Queries.GetProducts;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using Mapster;
using Shared.Core.API;
using Shared.Core.Validation.Result;

namespace Catalog.Tests.Integration.Application.UseCases.Products.Queries;

public class GetProductsQueryHandlerTest : IntegrationTest
{
    private readonly GetProductsQueryHandler _handler;

    public GetProductsQueryHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _handler = new GetProductsQueryHandler(ApplicationDbContext);
        ConfigureMapster();
    }

    private Product CreateTestProduct(string title, decimal price, Guid? categoryId = null)
    {
        var testProduct = Product.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create(title).Value,
            ProductDescription.Create("Desc").Value,
            ProductPrice.Create(price).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId != null ? CategoryId.Create(categoryId.Value).Value : null
        );
        testProduct.StockQuantity = StockQuantity.Create(5).Value;

        return testProduct;
    }

    private Category CreateTestCategory(Guid id) => Category.Create(
        CategoryId.Create(id).Value,
        CategoryName.Create("Category").Value,
        CategoryDescription.Create("Description").Value
    );

    [Fact]
    public async Task Handle_NoProducts_ReturnsNotFoundError()
    {
        // Arrange
        var query = new GetProductsQuery(new PaginationRequest(), new ProductFilterRequest());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e == Error.NotFound);
    }

    [Fact]
    public async Task Handle_ProductsExist_ReturnsPaginatedResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);
        ApplicationDbContext.Categories.Add(category);

        List<Product> products = [];
        for (int i = 1; i <= 5; i++)
        {
            var prod = CreateTestProduct($"Title #{i}", i, category.Id.Value);

            prod.StockQuantity = StockQuantity.Create(i * 2).Value;
            products.Add(prod);
            ApplicationDbContext.Products.Add(prod);
        }

        await ApplicationDbContext.SaveChangesAsync(default);

        var query = new GetProductsQuery(new PaginationRequest(), new ProductFilterRequest());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var page = result.Value;
        page.Data.Count().Should();
        var dtos = products
            .Adapt<List<ProductReadDto>>();
        page.Data.Should().BeEquivalentTo(dtos);
    }

    [Fact]
    public async Task Handle_FilterByTitle_ReturnsOnlyMatchingProducts()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = CreateTestCategory(categoryId);
        ApplicationDbContext.Categories.Add(category);

        List<Product> products = [];
        for (int i = 1; i <= 5; i++)
        {
            var prod = CreateTestProduct($"Title #{i}", i, i % 2 == 0 ? null : category.Id.Value);
            products.Add(prod);
            ApplicationDbContext.Products.Add(prod);
        }

        ApplicationDbContext.Products.AddRange(products);
        await ApplicationDbContext.SaveChangesAsync(default);

        var filterTitle = new ProductFilterRequest(Title: "Title #1");
        var filterCategory = new ProductFilterRequest(CategoryId: categoryId);
        var filterPrice = new ProductFilterRequest(MinPrice: 2, MaxPrice: 4);

        var queryTitle = new GetProductsQuery(new PaginationRequest(), filterTitle);
        var queryCategory = new GetProductsQuery(new PaginationRequest(), filterCategory);
        var queryPrice = new GetProductsQuery(new PaginationRequest(), filterPrice);

        // Act
        var resultTitle = await _handler.Handle(queryTitle, default);
        var resultCategory = await _handler.Handle(queryCategory, default);
        var resultPrice = await _handler.Handle(queryPrice, default);

        // Assert
        resultTitle.IsSuccess.Should().BeTrue();
        resultCategory.IsSuccess.Should().BeTrue();
        resultPrice.IsSuccess.Should().BeTrue();

        resultTitle.Value.Data.Should().ContainSingle(p => p.Title == "Title #1");
        resultCategory.Value.Data.Should().AllSatisfy(p =>
        {
            p.Category.Should().NotBeNull();
            p.Category!.Id.Should().Be(categoryId);
        });
        resultPrice.Value.Data.Should().HaveCount(3);
    }
}