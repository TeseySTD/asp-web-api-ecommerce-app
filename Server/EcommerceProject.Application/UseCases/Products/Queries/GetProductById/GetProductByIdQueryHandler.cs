using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Products;

namespace EcommerceProject.Application.UseCases.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, ProductReadDto>
{
    private readonly IProductsRepository _productsRepository;
    private readonly ICategoriesRepository _categoriesRepository;

    public GetProductByIdQueryHandler(IProductsRepository productsRepository,
        ICategoriesRepository categoriesRepository)
    {
        _productsRepository = productsRepository;
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Result<ProductReadDto>> Handle(GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productsRepository.FindById(request.Id, cancellationToken);
        if (product == null)
            return Error.NotFound;

        var category =  await _categoriesRepository.FindById(product.CategoryId, cancellationToken);

        var categoryDto = category == null
            ? null
            : new CategoryDto(
                Id: category.Id.Value,
                Name: category.Name.Value,
                Description: category.Description.Value
            );

        var response =
            new ProductReadDto(
                Id: product.Id.Value,
                Title: product.Title.Value,
                Description: product.Description.Value,
                Price: product.Price.Value,
                Quantity: product.StockQuantity.Value,
                Category: categoryDto 
            );

        return response;
    }
}