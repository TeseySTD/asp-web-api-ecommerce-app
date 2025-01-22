using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Ordering.Core.Models.Products;

public class Product : AggregateRoot<ProductId>
{
    private Product() : base(default!)
    {
    }

    private Product(ProductId id, ProductTitle title, ProductDescription description) : base(id)
    {
        Title = title;
        Description = description;
    }

    public ProductTitle Title { get; private set; }
    public ProductDescription Description { get; private set; }
    

    public static Product Create(ProductId id, ProductTitle title, ProductDescription description)
    {
        var product = new Product(id, title, description);
        return product;
    }

    public void Update(ProductTitle title, ProductDescription description,
        OrderItemPrice price)
    {
        Title = title;
        Description = description;
    }
}