using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.Entities;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.Entities;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryImage> CategoryImages { get; set; }
    public DbSet<Image> Images { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}