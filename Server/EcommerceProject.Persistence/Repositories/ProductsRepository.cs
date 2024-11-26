using EcommerceProject.Application.Abstractions.Interfaces.Repositories;
using EcommerceProject.Core.Models;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Persistence.Entities;
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
        var entity = new ProductEntity
        {
            Title = product.Title,
            Description = product.Description,
            Price = product.Price
        };
        await _context.Products.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Product>> Get()
    {
        return await _context.Products.Select(p => Product.Create(p.Title, p.Description, p.Price, new  p.Id))
                                        .AsNoTracking()
                                        .ToListAsync();
    }

    public async Task Update(Guid id, string title, string description, decimal price)
    {
        if(!await _context.Products.AnyAsync(p => p.Id == id))
            throw new Exception("Product not found, incorrect id");

        await _context.Products.Where(p => p.Id == id)
                                .ExecuteUpdateAsync(p => p
                                    .SetProperty(p => p.Title, title)
                                    .SetProperty(p => p.Description, description)
                                    .SetProperty(p => p.Price, price));
    }

    public async Task Delete(Guid id)
    {
        if(!await _context.Products.AnyAsync(p => p.Id == id))
            throw new Exception("Product not found, incorrect id");
        await _context.Products.Where(p => p.Id == id).ExecuteDeleteAsync();
    }

    public async Task<Product?> FindById(Guid id)
    {
        var product = await _context.Products.Where(p => p.Id == id)
                                            .AsNoTracking()
                                            .FirstOrDefaultAsync();

        return product != null ? Product.Create(product.Id, product.Title, product.Description, product.Price) : null;
    }
}
