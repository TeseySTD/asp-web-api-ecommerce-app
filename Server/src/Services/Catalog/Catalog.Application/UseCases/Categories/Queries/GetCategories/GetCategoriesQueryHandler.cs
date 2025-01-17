using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Select(c =>
                new CategoryDto(
                    c.Id.Value, c.Name.Value, c.Description.Value
                )
            )
            .AsNoTracking()
            .ToListAsync();

        if (!categories.Any())
            return Error.NotFound;
        return categories;
    }
}