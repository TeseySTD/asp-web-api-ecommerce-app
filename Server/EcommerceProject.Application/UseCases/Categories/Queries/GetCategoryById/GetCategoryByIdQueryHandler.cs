using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == request.Id)
            .Select(c =>
                new CategoryDto(
                    c.Id.Value, c.Name.Value, c.Description.Value
                )
            )
            .FirstOrDefaultAsync(cancellationToken);
        
        if(result == null)
            return Error.NotFound;
        
        return result;
    }
}