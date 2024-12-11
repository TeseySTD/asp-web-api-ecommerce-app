using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using MediatR;

namespace EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IProductsRepository _productsRepository;
    private readonly ICategoriesRepository _categoriesRepository;

    public CreateProductCommandHandler(IProductsRepository productsRepository,
        ICategoriesRepository categoriesRepository)
    {
        _productsRepository = productsRepository;
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if(!await _categoriesRepository.Exists(request.CategoryId, cancellationToken))
                return new Error("Category doesn't exist",
                    $"Category with id: {request.CategoryId.Value} does not exist");
            
            Product product;
            product = Product.Create(
                id: ProductId.Create(request.Value.Id).Value,
                title: ProductTitle.Create(request.Value.Title).Value,
                description: ProductDescription.Create(request.Value.Description).Value,
                price: ProductPrice.Create(request.Value.Price).Value,
                categoryId: request.CategoryId);
            product.StockQuantity = StockQuantity.Create(request.Value.Quantity);

            await _productsRepository.Add(product);
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure([new Error(e.Message, e.StackTrace ?? string.Empty)]);
        }
    }
}