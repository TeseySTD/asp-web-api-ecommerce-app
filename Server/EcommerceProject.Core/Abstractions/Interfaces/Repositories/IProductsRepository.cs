using System;
using EcommerceProject.Core.Models;

namespace EcommerceProject.Core.Abstractions.Interfaces.Repositories;

public interface IProductsRepository
{
    Task Add(Product product);
    Task<List<Product>> Get();
    Task Update(Guid id, string title, string description, decimal price);
    Task Delete(Guid productId);
    Task<Product?> FindById(Guid productId);
}