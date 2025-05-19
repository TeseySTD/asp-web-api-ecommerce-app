using Catalog.Core.Models.Categories.Entities;
using Catalog.Core.Models.Categories.Events;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Catalog.Core.Models.Categories;

public class Category : AggregateRoot<CategoryId>
{
    internal Category(CategoryId id, CategoryName name, CategoryDescription description) : base(id)
    {
        Name = name;
        Description = description;
    }
    
    public CategoryName Name { get; private set; }
    public CategoryDescription Description { get; private set; }
    public const int MaxImagesCount = 5; 
    public List<CategoryImage> Images { get; private set; } = new ();

    public static Category Create(CategoryId id, CategoryName name, CategoryDescription description)
    {
        var category = new Category(id, name, description);
        category.AddDomainEvent(new CategoryCreatedDomainEvent(category.Id));
        
        return category;
    }

    public static Category Create(CategoryName name, CategoryDescription description) =>
        Create(CategoryId.Create(Guid.NewGuid()).Value, name, description);

    public void Update(CategoryName name, CategoryDescription description)
    {
        Name = name;
        Description = description;
        
        AddDomainEvent(new CategoryUpdatedDomainEvent(this));
    }

    public void AddImage(Image image)
    {
        var categoryImage = CategoryImage.Create(image.Id, Id);
        if(Images.Count < MaxImagesCount)
            Images.Add(categoryImage);
    }

    public void RemoveImage(ImageId imageId)
    {
        Images.RemoveAll(i => i.Id == imageId);
        AddDomainEvent(new CategoryUpdatedDomainEvent(this));
    }
}