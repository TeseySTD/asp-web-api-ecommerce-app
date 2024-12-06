using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
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
        if(!await _productsRepository.Exists(ProductId.Create(request.Dto.Id), cancellationToken))
            return Result.Failure(Error.NotFound);
        try
        {
            await _productsRepository.Update(
                id: ProductId.Create(request.Dto.Id),
                title: ProductTitle.Create(request.Dto.Title),
                description: ProductDescription.Create(request.Dto.Description),
                price: ProductPrice.Create(request.Dto.Price),
                categoryId: null);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error(e.Message, e.StackTrace ?? string.Empty));
        }
    }
}