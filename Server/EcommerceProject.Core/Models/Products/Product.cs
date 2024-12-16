using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products.Events;
using EcommerceProject.Core.Models.Products.ValueObjects;

namespace EcommerceProject.Core.Models.Products;

public class Product : AggregateRoot<ProductId>
{
    private Product() : base(default!){}
    private Product(ProductId id, ProductTitle title, ProductDescription description, ProductPrice price,
        CategoryId? category) : base(id)
    {
        Title = title;
        Description = description;
        Price = price;
        CategoryId = category;
    }

    public ProductTitle Title { get; private set; }
    public ProductDescription Description { get; private set; }
    public CategoryId? CategoryId { get; private set; }
    public ProductPrice Price { get; private set; }
    public StockQuantity StockQuantity { get; set; } = default!;

    public bool IsInStock => StockQuantity.Value > 0;


    public static Product Create(ProductId id, ProductTitle title, ProductDescription description,
        ProductPrice price, CategoryId? categoryId)
    {
        var product = new Product(id, title, description, price, categoryId);
        product.AddDomainEvent(new ProductCreatedDomainEvent(id));
        return product;
    }

    public static Product Create(ProductTitle title, ProductDescription description, ProductPrice price,
        CategoryId? categoryId)
    {
        var id = ProductId.Create(Guid.NewGuid()).Value;
        return Create(id, title, description, price, categoryId);
    }
}