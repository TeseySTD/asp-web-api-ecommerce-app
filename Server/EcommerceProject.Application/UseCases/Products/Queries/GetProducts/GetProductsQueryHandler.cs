using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, GetProductsResponse>
{
    private readonly IProductsRepository _productsRepository;

    public GetProductsQueryHandler(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<Result<GetProductsResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _productsRepository.Get(cancellationToken);
        if (!products.Any())
            return Result<GetProductsResponse>.Failure([Error.NotFound]);
        else
        {
            var response = new GetProductsResponse(
                products.Select(p => new ProductDto(
                    Id: p.Id.Value,
                    Title: p.Title.Value,
                    Description: p.Description.Value,
                    Price: p.Price.Value,
                    Quantity: p.StockQuantity.Value
                )).ToList()
            );
            return Result<GetProductsResponse>.Success(response);
        }
    }
}