using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Products.Entities;
using Catalog.Core.Models.Products.Events;
using Catalog.Core.Models.Products.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Catalog.Core.Models.Products;

public class Product : AggregateRoot<ProductId>
{
    private Product() : base(default!)
    {
    }

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
    public const int MaxImagesCount = 5;
    public List<ProductImage> Images { get; private set; } = new ();
    public CategoryId? CategoryId { get; private set; }
    public Category? Category { get; private set; }
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

    public void Update(ProductTitle title, ProductDescription description,
        ProductPrice price, StockQuantity quantity, Category? category)
    {
        Title = title;
        Description = description;
        Price = price;
        StockQuantity = quantity;
        CategoryId = category is null ? null : category.Id;
        Category = category;
        
        AddDomainEvent(new ProductUpdatedDomainEvent(this));
    }

    public void AddImage(Image image)
    {
        var productImage = ProductImage.Create(image.Id, Id);
        if(Images.Count < MaxImagesCount)
            Images.Add(productImage);
    }  
        
    public void IncreaseQuantity(uint quantity) => StockQuantity = StockQuantity.Create(StockQuantity.Value + quantity);
    public void DecreaseQuantity(uint quantity)
    {
        if(StockQuantity.Value >= quantity)
            StockQuantity = StockQuantity.Create(StockQuantity.Value - quantity);
        else
            throw new ArgumentException("The quantity provided is greater than the quantity of product."); 
    }
}