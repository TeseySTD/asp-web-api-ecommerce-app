using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Persistence.Repositories;

public class CategoriesRepository : ICategoriesRepository
{
    private readonly StoreDbContext _context;
    public CategoriesRepository(StoreDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Category>> Get(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Category?> FindById(CategoryId id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Result> Add(Category category, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Categories.AddAsync(category, cancellationToken);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception e)
        {
            return Result.Failure(new Error(e.Message, e.StackTrace ?? string.Empty));
        }
        
    }

    public async Task<Result> Update(CategoryId id, Category category, CancellationToken cancellationToken = default)
    {
        if(!await _context.Categories.AnyAsync(p => p.Id == id, cancellationToken))
            return Error.NotFound;

        try
        {
            await _context.Categories
                .Where(c => c.Id == id)
                    .ExecuteUpdateAsync(c => c
                        .SetProperty(p => p.Name, category.Name)
                        .SetProperty(p => p.Description, category.Description));
            return Result.Success();
        }
        catch (Exception e)
        {
            return new Error(e.Message, e.StackTrace ?? string.Empty);
        }
        
    }

    public async Task<Result> Delete(CategoryId id, CancellationToken cancellationToken)
    {
        if(!await _context.Categories.AnyAsync(c => c.Id == id))
            return Error.NotFound;
        
        await _context.Categories.Where(p => p.Id == id).ExecuteDeleteAsync();
        return Result.Success();
    }
}