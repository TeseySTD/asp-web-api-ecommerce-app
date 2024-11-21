using System;
using EcommerceProject.Core.Models;

namespace EcommerceProject.Core.Abstractions.Interfaces.Services;

public interface IProductService
{
    Task AddProduct(Product product);
    Task<List<Product>> GetAllProducts();
    Task UpdateProduct(Product product);
    Task DeleteProduct(Guid productId);
    Task<Product?> FindProductById(Guid productId);
}
