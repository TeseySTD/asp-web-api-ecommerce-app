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

    [Fact]
    public async Task WhenNoProducts_ThenReturnsNotFoundError()
    {
        // Arrange
        var query = new GetProductsQuery(new PaginationRequest());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(e => e == Error.NotFound);
    }

    [Fact]
    public async Task WhenProductsExist_ThenReturnsPaginatedResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Category").Value,
            CategoryDescription.Create("Description").Value
        );
        ApplicationDbContext.Categories.Add(category);

        List<Product> products = [];
        for (int i = 1; i <= 5; i++)
        {
            var prod = Product.Create(
                ProductId.Create(Guid.NewGuid()).Value,
                ProductTitle.Create($"Title #{i}").Value,
                ProductDescription.Create($"Description #{i}").Value,
                ProductPrice.Create(i * 10m).Value,
                category.Id
            );
            prod.StockQuantity = StockQuantity.Create(i * 2).Value;
            products.Add(prod);
            ApplicationDbContext.Products.Add(prod);
        }

        await ApplicationDbContext.SaveChangesAsync(default);

        var query = new GetProductsQuery(new PaginationRequest());
        
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
}