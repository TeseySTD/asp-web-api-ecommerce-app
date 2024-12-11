using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductsRepository _productsRepository;
    private readonly ICategoriesRepository _categoriesRepository;

    public GetProductsQueryHandler(IProductsRepository productsRepository, ICategoriesRepository categoriesRepository)
    {
        _productsRepository = productsRepository;
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Result<GetProductsResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productsRepository.Get(cancellationToken);
        if (!products.Any())
            return Result<GetProductsResponse>.Failure([Error.NotFound]);


        var productDtos = new List<ProductDto>();

        foreach (var product in products)
        {
            var category = await _categoriesRepository.FindById(product.CategoryId, cancellationToken);
            var categoryDto = category == null ? null
                : new CategoryDto(
                    Id: category.Id.Value,
                    Name: category.Name.Value,
                    Description: category.Description.Value
                );

            productDtos.Add(new ProductDto(
                Id: product.Id.Value,
                Title: product.Title.Value,
                Description: product.Description.Value,
                Price: product.Price.Value,
                Quantity: product.StockQuantity.Value,
                Category: categoryDto
            ));
        }

        var response = new GetProductsResponse(productDtos);

        return Result<GetProductsResponse>.Success(response);

    }
}