using System.Reflection;
using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Products;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Domain.Classes;

namespace Catalog.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }

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