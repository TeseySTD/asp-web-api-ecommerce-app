using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.UseCases.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private IProductsRepository _productsRepository;

    public UpdateProductCommandHandler(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if(!await _productsRepository.Exists(ProductId.Create(request.Value.Id).Value, cancellationToken))
            return Result.Failure([Error.NotFound]);

        return await _productsRepository.Update(
                id: ProductId.Create(request.Value.Id).Value,
                title: ProductTitle.Create(request.Value.Title).Value,
                description: ProductDescription.Create(request.Value.Description).Value,
                price: ProductPrice.Create(request.Value.Price).Value,
                categoryId: request.CategoryId);

    }
}