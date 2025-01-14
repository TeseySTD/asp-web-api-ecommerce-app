using Ordering.Core.Models.Products.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Ordering.Core.Models.Products;

public class Product : AggregateRoot<ProductId>
{
    private Product() : base(default!)
    {
    }

    private Product(ProductId id, ProductTitle title, ProductDescription description, ProductPrice price) : base(id)
    {
        Title = title;
        Description = description;
        Price = price;
    }

    public ProductTitle Title { get; private set; }
    public ProductDescription Description { get; private set; }
    public ProductPrice Price { get; private set; }
    

    public static Product Create(ProductId id, ProductTitle title, ProductDescription description,
        ProductPrice price)
    {
        var product = new Product(id, title, description, price);
        return product;
    }

    public static Product Create(ProductTitle title, ProductDescription description, ProductPrice price)
    {
        var id = ProductId.Create(Guid.NewGuid()).Value;
        return Create(id, title, description, price);
    }

    public void Update(ProductTitle title, ProductDescription description,
        ProductPrice price)
    {
        Title = title;
        Description = description;
        Price = price;
    }
}