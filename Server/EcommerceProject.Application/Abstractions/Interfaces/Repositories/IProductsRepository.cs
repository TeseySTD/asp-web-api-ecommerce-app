using System;
using EcommerceProject.Core.Models;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.Abstractions.Interfaces.Repositories;

public interface IProductsRepository
{
    Task Add(Product product);
    Task<List<Product>> Get();
    Task Update(ProductId id, ProductTitle title, ProductDescription description, ProductPrice price, CategoryId category);
    Task Delete(ProductId productId);
    Task<Product?> FindById(ProductId productId);
}