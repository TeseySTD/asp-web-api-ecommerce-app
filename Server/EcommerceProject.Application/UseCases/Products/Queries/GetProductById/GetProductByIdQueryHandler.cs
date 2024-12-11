using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;

namespace EcommerceProject.Application.UseCases.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, GetProductByIdResponse>
{
    private readonly IProductsRepository _productsRepository;
    private readonly ICategoriesRepository _categoriesRepository;

    public GetProductByIdQueryHandler(IProductsRepository productsRepository,
        ICategoriesRepository categoriesRepository)
    {
        _productsRepository = productsRepository;
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Result<GetProductByIdResponse>> Handle(GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productsRepository.FindById(request.Id, cancellationToken);
        if (product == null)
            return Result<GetProductByIdResponse>.Failure([Error.NotFound]);
        else
        {
            var category = await _categoriesRepository.FindById(product.CategoryId, cancellationToken);
            if (category == null)
                return new Error("Category doesn't exist", $"Category with id: {product.CategoryId.Value} does not exist");
            

            var response = new GetProductByIdResponse(
                new ProductDto(
                    Id: product.Id.Value,
                    Title: product.Title.Value,
                    Description: product.Description.Value,
                    Price: product.Price.Value,
                    Quantity: product.StockQuantity.Value,
                    Category: new CategoryDto(
                        Id: category.Id.Value,
                        Name: category.Name.Value,
                        Description: category.Description.Value
                    )
                )
            );

            return response;
        }
    }
}