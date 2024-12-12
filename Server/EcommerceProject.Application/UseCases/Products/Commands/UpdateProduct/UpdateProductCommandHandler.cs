using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories.ValueObjects;
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
        return await _productsRepository.Update(
            id: ProductId.Create(request.Value.Id).Value,
            title: ProductTitle.Create(request.Value.Title).Value,
            description: ProductDescription.Create(request.Value.Description).Value,
            price: ProductPrice.Create(request.Value.Price).Value,
            quantity: StockQuantity.Create(request.Value.Quantity),
            categoryId: request.Value.CategoryId == null ? null : CategoryId.Create((Guid)request.Value.CategoryId).Value);
    }
}