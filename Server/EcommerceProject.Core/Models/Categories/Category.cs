using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Core.Models.Categories;

public class Category : AggregateRoot<CategoryId>
{
    public const int MaxNameLength = 50;
    public const int MinDescriptionLength = 3;
    public const int MaxDescriptionLength = 200;

    internal Category(CategoryId id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
    }

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or whitespace", nameof(Name));
            if (value.Length > MaxNameLength)
                throw new ArgumentException($"Name must be less than {MaxNameLength} symbols", nameof(Name));
            _name = value;
        }
    }

    private string _description = string.Empty;

    public string Description
    {
        get => _description;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (value.Length < MinDescriptionLength || value.Length > MaxDescriptionLength)
                throw new ArgumentException(
                    $"Description must be less then {MaxDescriptionLength} symbols and more than {MinDescriptionLength} symbols",
                    nameof(value));

            _description = value;
        }
    }
}