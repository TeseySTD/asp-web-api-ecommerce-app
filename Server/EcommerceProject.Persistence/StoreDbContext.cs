using System;
using EcommerceProject.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace EcommerceProject.Persistence;

public class StoreDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; }
    public DbSet<CategoryEntity> Categories { get; set; }
    public DbSet<OrderEntity> Orders { get; set; }
    public DbSet<OrderItemEntity> OrderItems { get; set; }
    public DbSet<UserEntity> Users { get; set; }

    public StoreDbContext(DbContextOptions options) : base(options)
    {
    }
}
