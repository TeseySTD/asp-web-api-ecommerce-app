using System;
using EcommerceProject.Core.Abstractions.Classes;

namespace EcommerceProject.Core.Models.Products.Entities;

public class Category : Entity<Guid>
{
    public const int MaxNameLength = 50;
    public const int MinDescriptionLength = 3;
    public const int MaxDescriptionLength = 200;

    public Category(Guid id) : base(id)
    {
    }

    private string _name = string.Empty;
    public string Name
    {
        get { return _name; }
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
        get { return _description; }
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(nameof(value));
            if (value.Length < MinDescriptionLength || value.Length > MaxDescriptionLength)
                throw new ArgumentException($"Description must be less then {MaxDescriptionLength} symbols and more than {MinDescriptionLength} symbols", nameof(value));

            _description = value;
        }
    }
}
