using EcommerceProject.Application.Abstractions.Interfaces.Repositories;
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

    public async Task Add(Product product)
    {
        var entity = Product.Create(
            id: product.Id,
            title: product.Title,
            description: product.Description,
            price: product.Price,
            categoryId: product.CategoryId
        );
        await _context.Products.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> Get()
    {
        return await _context.Products
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task Update(ProductId id, ProductTitle title, ProductDescription description, ProductPrice price, CategoryId category)
    {
        if(!await _context.Products.AnyAsync(p => p.Id == id))
            throw new Exception("Product not found, incorrect id");

        await _context.Products
            .Where(p => p.Id == id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(p => p.Title, title)
                    .SetProperty(p => p.Description, description)
                    .SetProperty(p => p.Price, price)
                    .SetProperty(p => p.CategoryId, category));
    }

    public async Task Delete(ProductId id)
    {
        if(!await _context.Products.AnyAsync(p => p.Id == id))
            throw new Exception("Product not found, incorrect id");
        await _context.Products.Where(p => p.Id == id).ExecuteDeleteAsync();
    }

    public async Task<Product?> FindById(ProductId id)
    {
        var product = await _context.Products.Where(p => p.Id == id)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();

        return product;
    }
}
