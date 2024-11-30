using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IProductsRepository _productsRepository;

    public CreateProductCommandHandler(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Product product;
            product = Product.Create(
                title: ProductTitle.Of(request.Value.Title),
                description: ProductDescription.Of(request.Value.Description),
                price: ProductPrice.Of(request.Value.Price),
                null);
            product.StockQuantity = StockQuantity.Of(request.Value.Quantity);
            
            await _productsRepository.Add(product);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error(e.Message, e.StackTrace ?? string.Empty));
        }
    }
}