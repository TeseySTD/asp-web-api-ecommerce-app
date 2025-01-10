using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
            return new Error("Product not found, incorrect id",
                $"Product not found, incorrect id:{request.ProductId.Value}");

        await _context.Products.Where(p => p.Id == request.ProductId).ExecuteDeleteAsync();
        return Result.Success();
    }
}