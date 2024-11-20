using System;
using EcommerceProject.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Persistence;

public class StoreDbContext : DbContext
{
    public DbSet<ProductEntity> Products { get; set; }

    public StoreDbContext(DbContextOptions options) : base(options)
    {
    }
}
