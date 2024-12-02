namespace EcommerceProject.Core.Models.Categories.ValueObjects;

public record CategoryId
{
    private CategoryId(Guid categoryId)
    {
        Value = categoryId;
    }

    public Guid Value { get; set; }

    public static CategoryId Create(Guid categoryId)
    {
        return new CategoryId(categoryId);
    }
}