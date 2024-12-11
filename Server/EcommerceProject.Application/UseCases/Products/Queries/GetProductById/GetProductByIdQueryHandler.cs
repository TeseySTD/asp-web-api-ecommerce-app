using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler : IQueryHandler<GetProductByIdQuery, GetProductByIdResponse>
{
    private readonly IProductsRepository _productsRepository;

    public GetProductByIdQueryHandler(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<Result<GetProductByIdResponse>> Handle(GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productsRepository.FindById(request.Id, cancellationToken);
        if (product == null)
            return Result<GetProductByIdResponse>.Failure([Error.NotFound]);
        else
        {
            var response = new GetProductByIdResponse(
                new ProductDto(
                    Id: product.Id.Value,
                    Title: product.Title.Value,
                    Description: product.Description.Value,
                    Price: product.Price.Value,
                    Quantity: product.StockQuantity.Value
                )
            );
            
            return Result<GetProductByIdResponse>.Success(response);
        }
    }
}