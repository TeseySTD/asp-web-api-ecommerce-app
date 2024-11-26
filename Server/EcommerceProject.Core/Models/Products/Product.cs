using EcommerceProject.Core.Abstractions.Classes;
using EcommerceProject.Core.Models.Products.Entities;
using EcommerceProject.Core.Models.Products.Events;

namespace EcommerceProject.Core.Models.Products;
public class Product : AggregateRoot<Guid>
{
    public const int MaxTitleLength = 200;
    public const int MinTitleLength = 2;
    public const int MaxDescriptionLength = 500;
    public const int MinDescriptionLength = 10;
    public const decimal MinPrice = 0.1m;
    public const decimal MaxPrice = 1000000.0m;

    private string _title = string.Empty;
    private string _description = string.Empty;
    private decimal _price;
    private decimal _stockQuantity = 0;
    private decimal _discountInPercent = 0;
    private Category _category = null!;

    private Product(Guid id, string title, string description, decimal price, Category category) : base(id)
    {
        Title = title;
        Description = description;
        Price = price;
        Category = category;    
    }
    public string Title
    {
        get { return _title; }
        set
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Title));
            if (value.Length > MaxTitleLength || value.Length < MinTitleLength)
                throw new ArgumentException($"Title must be between {MinTitleLength} and {MaxTitleLength} characters", nameof(Title));
            _title = value;
        }
    }
    public string Description
    {
        get => _description;
        set
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Description));
            if (value.Length > MaxDescriptionLength || value.Length < MinDescriptionLength)
                throw new ArgumentException($"Description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters", nameof(Description));
            _description = value;
        }
    }
    public decimal Price
    {
        get => _price;
        set
        {
            if (value < MinPrice || value > MaxPrice) throw new ArgumentException($"Price must be between {MinPrice} and {MaxPrice}", nameof(Price));
            _price = value;
        }
    }

    public decimal StockQuantity
    {
        get => _stockQuantity;
        set
        {
            if (value < 0) throw new ArgumentException($"Stock quantity must be positive", nameof(StockQuantity));
            _stockQuantity = value;
        }
    }

    public decimal DiscountInPercent
    {
        get => _discountInPercent;
        set
        {
            if (value < 0 || value > 100) throw new ArgumentException($"Discount must be between 0 and 100", nameof(DiscountInPercent));
            _discountInPercent = value;
        }
    }

    public bool IsInStock => StockQuantity > 0;

    public Category Category
    {
        get => _category;
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(Category));
            _category = value;
        }
    }

    public static Product Create(string title, string description, decimal price, Category category, Guid id = new Guid())
    {
        var product = new Product(id, title, description, price, category);
        product.AddDomainEvent(new ProductCreatedDomainEvent(id));
        return product;
    }
}