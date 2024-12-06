using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IProductsRepository _productsRepository;

    public DeleteProductCommandHandler(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        if (!await _productsRepository.Exists(request.ProductId, cancellationToken))
            return Result.Failure(Error.NotFound);
        else
        {
            await _productsRepository.Delete(request.ProductId);
            return Result.Success();
        }
    }
}