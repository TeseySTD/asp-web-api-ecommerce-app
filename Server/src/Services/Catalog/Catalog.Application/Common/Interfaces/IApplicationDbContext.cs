using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}