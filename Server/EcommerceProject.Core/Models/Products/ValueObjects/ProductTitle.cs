namespace EcommerceProject.Core.Models.Products.ValueObjects;

public record ProductTitle
{
    public const int MaxTitleLength = 200;
    public const int MinTitleLength = 2;
    public string Value { get; }

    protected ProductTitle(string title)
    {
        Value = title;
    }

    public static ProductTitle Of(string title){
        ArgumentNullException.ThrowIfNull(title, nameof(title));
        if(title.Length > MaxTitleLength || title.Length < MinTitleLength)
            throw new ArgumentException($"Product title must be between {MinTitleLength} and {MaxTitleLength} characters.");
        return new ProductTitle(title);
    }

}
