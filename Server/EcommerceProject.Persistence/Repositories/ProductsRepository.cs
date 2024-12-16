using System.Linq.Expressions;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Persistence.Repositories;

public class ProductsRepository : IProductsRepository
{
    private readonly StoreDbContext _context;
    public ProductsRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Add(Product product)
    {
        var result = Result.TryFail()
            .CheckError(await _context.Products.AnyAsync(p => p.Id == product.Id),
                new Error("Product already exists", $"Product already exists, incorrect id:{product.Id}"))
            .CheckError(!await _context.Categories.AnyAsync(p => p.Id == product.CategoryId),
                new Error("Category not found", $"Category not found, incorrect id:{product.CategoryId}"))
            .Build();
        
        if(result.IsSuccess)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }
        return result;
    }

    public async Task<IEnumerable<Product>> Get(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<Result> Update(ProductId id, ProductTitle title, ProductDescription description, ProductPrice price, StockQuantity quantity, CategoryId categoryId)
    {
        var result = Result.TryFail()
            .CheckError(!await _context.Products.AnyAsync(p => p.Id == id),
                new Error("Product not found", $"Product not found, incorrect id:{id}"))
            .CheckError(!await _context.Categories.AnyAsync(p => p.Id == categoryId) && categoryId != null,
                new Error("Category not found", $"Category not found, incorrect id:{categoryId}"))
            .Build();
        if(result.IsFailure)
            return result;
        
        await _context.Products
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(p => p
                .SetProperty(pr => pr.Title, title)
                .SetProperty(pr => pr.Description, description)
                .SetProperty(pr => pr.Price, price)
                .SetProperty(pr => pr.StockQuantity, quantity)
                .SetProperty(pr => pr.CategoryId, categoryId));
        
        return Result.Success();
    }

    public async Task<Result> Delete(ProductId id)
    {
        if(!await _context.Products.AnyAsync(p => p.Id == id))
            return new Error("Product not found, incorrect id", $"Product not found, incorrect id:{id}");
        
        await _context.Products.Where(p => p.Id == id).ExecuteDeleteAsync();
        return Result.Success();
    }

    public async Task<Product?> FindById(ProductId id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.Where(p => p.Id == id)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync(cancellationToken);

        return product;
    }

    public async Task<bool> Exists(ProductId id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }

    public async Task<Result<IEnumerable<Product>>> SelectWithCondition(Expression<Func<Product, bool>> condition,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _context.Products.Where(condition).ToListAsync(cancellationToken);
            return result;
        }
        catch (Exception e)
        {
            return new Error("Error occured while selecting products", $"{e.Message} \n {e.StackTrace}");
        }
    }
}
