using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand>
{
    private IProductsRepository _productsRepository;

    public UpdateProductCommandHandler(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if(!await _productsRepository.Exists(ProductId.Of(request.dto.Id), cancellationToken))
            return Result.Failure(Error.NotFound);
        try
        {
            await _productsRepository.Update(
                id: ProductId.Of(request.dto.Id),
                title: ProductTitle.Of(request.dto.Title),
                description: ProductDescription.Of(request.dto.Description),
                price: ProductPrice.Of(request.dto.Price),
                categoryId: null);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error(e.Message, e.StackTrace ?? string.Empty));
        }
    }
}