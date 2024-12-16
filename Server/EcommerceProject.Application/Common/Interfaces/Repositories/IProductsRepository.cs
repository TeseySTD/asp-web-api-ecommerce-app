using System.Linq.Expressions;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Application.Common.Interfaces.Repositories;

public interface IProductsRepository
{
    Task<IEnumerable<Product>> Get(CancellationToken cancellationToken);
    Task<Product?> FindById(ProductId productId, CancellationToken cancellationToken);
    Task<Result> Add(Product product);
    Task<Result> Update(ProductId id, ProductTitle title, ProductDescription description, ProductPrice price, StockQuantity quantity,  CategoryId categoryId);
    Task<Result> Delete(ProductId productId);
    Task<bool> Exists(ProductId productId, CancellationToken cancellationToken);
    Task<Result<IEnumerable<Product>>> SelectWithCondition(Expression<Func<Product, bool>> condition, CancellationToken cancellationToken);
}