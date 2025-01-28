using System.Reflection;
using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.Entities;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Domain.Classes;

namespace Catalog.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<CategoryImage> CategoryImages { get; set; }
    public DbSet<Image> Images { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}