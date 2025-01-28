using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Catalog.Core.Models.Categories.Entities;

public class CategoryImage : Entity<ImageId>
{
    public CategoryId CategoryId { get; set; }

    private CategoryImage(ImageId id, CategoryId categoryId) : base(id)
    {
        CategoryId = categoryId;
    }

    public static CategoryImage Create(ImageId id, CategoryId categoryId)
    {
        return new(id, categoryId);
    }
}