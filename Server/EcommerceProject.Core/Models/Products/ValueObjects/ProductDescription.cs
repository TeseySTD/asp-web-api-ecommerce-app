namespace EcommerceProject.Core.Models.Products.ValueObjects;

public record ProductDescription
{
    public const int MaxDescriptionLength = 500;
    public const int MinDescriptionLength = 10;
    public string Value { get; private set; }
    
    private ProductDescription(string description)
    {   
        Value = description;
    }
    
    public static ProductDescription Create(string description){
        ArgumentNullException.ThrowIfNull(description, nameof(description));
        if(description.Length < MinDescriptionLength || description.Length > MaxDescriptionLength)
            throw new ArgumentException($"Product description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters.");
        return new ProductDescription(description);
    }

}
