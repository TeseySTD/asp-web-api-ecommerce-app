using EcommerceProject.Core.Models.Products;

namespace EcommerceProject.Application.Common.Interfaces.Services;

public interface IProductService
{
    Task AddProduct(Product product);
    Task<List<Product>> GetAllProducts();
    Task UpdateProduct(Product product);
    Task DeleteProduct(Guid productId);
    Task<Product?> FindProductById(Guid productId);
}
