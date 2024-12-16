using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Core.Models.Categories;

public class Category : AggregateRoot<CategoryId>
{
    internal Category(CategoryId id, CategoryName name, CategoryDescription description) : base(id)
    {
        Name = name;
        Description = description;
    }
    
    public CategoryName Name { get; private set; }
    public CategoryDescription Description { get; private set; }
    
    public static Category Create(CategoryId id, CategoryName name, CategoryDescription description) => new (id, name, description);
    public static Category Create(CategoryName name, CategoryDescription description) => new(CategoryId.Create(Guid.NewGuid()).Value, name, description);
}