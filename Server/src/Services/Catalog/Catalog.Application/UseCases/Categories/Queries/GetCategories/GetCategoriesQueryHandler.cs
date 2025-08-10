using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Shared.Core.API;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, PaginatedResult<CategoryReadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<CategoryReadDto>>> Handle(GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;

        var categories = await _context.Categories
            .ProjectToType<CategoryReadDto>()
            .AsNoTracking()
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync();

        if (!categories.Any())
            return Error.NotFound;
        return new PaginatedResult<CategoryReadDto>(pageIndex, pageSize, categories);
    }
}