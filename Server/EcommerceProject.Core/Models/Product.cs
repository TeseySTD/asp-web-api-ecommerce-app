namespace EcommerceProject.Core.Models;
public class Product
{
    public const int MAX_TITLE_LENGTH = 200;
    public const int MIN_TITLE_LENGTH = 2;
    public const int MAX_DESCRIPTION_LENGTH = 500;
    public const int MIN_DESCRIPTION_LENGTH = 10;
    public const decimal MIN_PRICE = 0.1m;
    public const decimal MAX_PRICE = 1000000.0m;

    private string _title = string.Empty;
    private string _description = string.Empty;
    private decimal _price;

    private Product(Guid id, string title, string description, decimal price)
    {
        Id = id;
        Title = title;
        Description = description;
        Price = price;
    }
    public Guid Id { get; private set; }
    public string Title
    {
        get { return _title; }
        set
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Title));
            if (value.Length > MAX_TITLE_LENGTH || value.Length < MIN_TITLE_LENGTH) throw new ArgumentException($"Title must be between {MIN_TITLE_LENGTH} and {MAX_TITLE_LENGTH} characters", nameof(Title));
            _title = value;
        }
    }
    public string Description
    {
        get => _description;
        set
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(Description));
            if (value.Length > MAX_DESCRIPTION_LENGTH || value.Length < MIN_DESCRIPTION_LENGTH)
                throw new ArgumentException($"Description must be between {MIN_DESCRIPTION_LENGTH} and {MAX_DESCRIPTION_LENGTH} characters", nameof(Description));
            _description = value;
        }
    }
    public decimal Price
    {
        get => _price;
        set
        {
            if(value < MIN_PRICE || value > MAX_PRICE) throw new ArgumentException($"Price must be between {MIN_PRICE} and {MAX_PRICE}", nameof(Price));
            _price = value;
        }
    }

    public static Product Create(string title, string description, decimal price)
    {
        return new Product(new Guid(), title, description, price);
    }
    public static Product Create(Guid id, string title, string description, decimal price)
    {
        return new Product(id, title, description, price);
    }
}