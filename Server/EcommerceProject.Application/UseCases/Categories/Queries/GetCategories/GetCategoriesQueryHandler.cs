using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(ICategoriesRepository categoriesRepository, IApplicationDbContext context)
    {
        _categoriesRepository = categoriesRepository;
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