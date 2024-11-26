using EcommerceProject.Application.Abstractions.Interfaces.Repositories;
using EcommerceProject.Application.Abstractions.Interfaces.Services;
using EcommerceProject.Core.Models.Products;

namespace EcommerceProject.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductsRepository _productsRepository;
    public ProductService(IProductsRepository productsRepository)
    {
        _productsRepository = productsRepository;
    }
    public async Task AddProduct(Product product)
    {
        await _productsRepository.Add(product);
    }

    public async Task DeleteProduct(Guid productId)
    {
        await _productsRepository.Delete(productId);
    }

    public async Task<Product?> FindProductById(Guid productId)
    {
        return await _productsRepository.FindById(productId);
    }

    public async Task<List<Product>> GetAllProducts()
    {
        return await _productsRepository.Get();
    }

    public Task UpdateProduct(Product product)
    {
        return _productsRepository.Update(product.Id, product.Title, product.Description, product.Price);
    }
}
