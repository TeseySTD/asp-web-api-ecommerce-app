namespace EcommerceProject.Core.Models.Products.ValueObjects;

public record ProductId
{
    public Guid Value { get; init; }

    private ProductId(Guid value)
    {
        Value = value;
    }
    public static ProductId Of(Guid productId)
    {
        return new ProductId(productId);
    }
}

