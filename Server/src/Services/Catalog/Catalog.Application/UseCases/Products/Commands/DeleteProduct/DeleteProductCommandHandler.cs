using Catalog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.Auth;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public DeleteProductCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var result = await Result.Try()
            .Check(!await _context.Products.AnyAsync(p => p.Id == request.ProductId),
                new ProductNotFoundError(request.ProductId.Value))
            .DropIfFail()
            .CheckAsync(async () => !await _context.Products
                .AnyAsync(p => p.Id == request.ProductId && p.SellerId == request.SellerId) && request.Role != UserRole.Admin,
                new CustomerMismatchError(request.SellerId.Value))
            .BuildAsync();
        
        if(result.IsFailure)
            return result;

        await _context.Products.Where(p => p.Id == request.ProductId).ExecuteDeleteAsync();
        await _cache.RemoveAsync($"product-{request.ProductId.Value}");

        return Result.Success();
    }

    public sealed record ProductNotFoundError(Guid ProductId) :
        Error("Product not found, incorrect id",
            $"Product not found, incorrect id:{ProductId}");
    public sealed record CustomerMismatchError(Guid SellerId) : Error("You can`t delete this product!",
        $"Your id {SellerId} doesn’t match with seller’s id in product.");
}