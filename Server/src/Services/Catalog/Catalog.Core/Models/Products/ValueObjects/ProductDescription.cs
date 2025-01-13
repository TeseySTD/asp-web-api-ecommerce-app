using Shared.Core.Validation;

namespace Catalog.Core.Models.Products.ValueObjects;

public record ProductDescription
{
    public const int MaxDescriptionLength = 500;
    public const int MinDescriptionLength = 10;
    public string Value { get; private set; }
    
    private ProductDescription(string description)
    {   
        Value = description;
    }
    
    public static Result<ProductDescription> Create(string description){
        var result = Result<ProductDescription>.TryFail()
            .CheckError(string.IsNullOrEmpty(description),
                new Error("Description is required", nameof(description)))
            .CheckError(description.Length < MinDescriptionLength || description.Length > MaxDescriptionLength,
                new Error("Product description is out of length",
                    $"Product description must be between {MinDescriptionLength} and {MaxDescriptionLength} characters."))
            .Build();
            
        if(result.IsFailure)
            return result;
        return new ProductDescription(description);
    }

}
