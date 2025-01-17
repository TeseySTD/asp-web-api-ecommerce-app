using Microsoft.EntityFrameworkCore;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Products;

namespace Ordering.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Order> Orders { get; set; }
    DbSet<OrderItem> OrderItems { get; set; }
    DbSet<Product> Products { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}