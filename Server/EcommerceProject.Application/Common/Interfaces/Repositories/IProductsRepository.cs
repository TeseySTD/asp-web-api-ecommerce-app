using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.Common.Interfaces.Repositories;

public interface IProductsRepository
{
    Task Add(Product product);
    Task<List<Product>> Get(CancellationToken cancellationToken);
    Task Update(ProductId id, ProductTitle title, ProductDescription description, ProductPrice price, CategoryId categoryId);
    Task Delete(ProductId productId);
    Task<Product?> FindById(ProductId productId, CancellationToken cancellationToken);
    Task<bool> Exists(ProductId productId, CancellationToken cancellationToken);
}