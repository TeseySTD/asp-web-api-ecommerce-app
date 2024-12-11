﻿using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;

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
                title: ProductTitle.Create(request.Value.Title).Value,
                description: ProductDescription.Create(request.Value.Description).Value,
                price: ProductPrice.Create(request.Value.Price).Value,
                null);
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