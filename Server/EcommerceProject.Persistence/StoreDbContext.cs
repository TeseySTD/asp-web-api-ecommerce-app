using System.Reflection;
using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Persistence;

public class StoreDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public StoreDbContext(DbContextOptions options) : base(options)
    {
    }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
