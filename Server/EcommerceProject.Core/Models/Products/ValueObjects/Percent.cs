namespace EcommerceProject.Core.Models.Products.ValueObjects;

public record Percent
{
    public decimal Value { get; init; }
    protected Percent(decimal percent) => Value = percent;
    public static Percent Create(decimal percent){
        if(percent < 0 || percent > 100)
            throw new ArgumentOutOfRangeException(nameof(Value));
        return new Percent(percent);
    }
}
