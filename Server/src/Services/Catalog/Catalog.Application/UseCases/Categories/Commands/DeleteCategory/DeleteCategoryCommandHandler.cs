using Catalog.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;

namespace Catalog.Application.UseCases.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        if(!await _context.Categories.AnyAsync(c => c.Id == request.Id))
            return Error.NotFound;
        
        await _context.Categories.Where(p => p.Id == request.Id).ExecuteDeleteAsync();
        return Result.Success();
    }
}