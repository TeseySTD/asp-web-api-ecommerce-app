using Catalog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
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
        if (!await _context.Products.AnyAsync(p => p.Id == request.ProductId))
            return new ProductNotFoundError(request.ProductId.Value);

        await _context.Products.Where(p => p.Id == request.ProductId).ExecuteDeleteAsync();
        await _cache.RemoveAsync($"product-{request.ProductId.Value}");
        
        return Result.Success();
    }
    
    public sealed record ProductNotFoundError(Guid ProductId) : Error("Product not found, incorrect id",
                                                                                      $"Product not found, incorrect id:{ProductId}");
}