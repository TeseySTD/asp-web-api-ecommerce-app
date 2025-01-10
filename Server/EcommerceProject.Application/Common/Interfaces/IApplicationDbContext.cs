using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.Common.Interfaces;

public interface IApplicationDbContext 
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}