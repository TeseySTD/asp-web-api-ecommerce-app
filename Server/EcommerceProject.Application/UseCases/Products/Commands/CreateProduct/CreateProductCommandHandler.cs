using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        Product product = Product.Create(
            id: ProductId.Create(request.Value.Id).Value,
            title: ProductTitle.Create(request.Value.Title).Value,
            description: ProductDescription.Create(request.Value.Description).Value,
            price: ProductPrice.Create(request.Value.Price).Value,
            categoryId: CategoryId.Create(request.Value.CategoryId).Value
        );

        product.StockQuantity = StockQuantity.Create(request.Value.Quantity);

        return await Add(product);
    }

    private async Task<Result> Add(Product product, CancellationToken cancellationToken = default)
    {
        var result = Result.TryFail()
            .CheckError(await _context.Products.AnyAsync(p => p.Id == product.Id),
                new Error("Product already exists", $"Product already exists, incorrect id:{product.Id}"))
            .CheckError(!await _context.Categories.AnyAsync(p => p.Id == product.CategoryId),
                new Error("Category not found", $"Category not found, incorrect id:{product.CategoryId}"))
            .Build();

        if (result.IsSuccess)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }
}