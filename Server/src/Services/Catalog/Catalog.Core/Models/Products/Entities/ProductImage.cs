using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Catalog.Core.Models.Products.Entities;

public class ProductImage : Entity<ImageId>
{
    public ProductId ProductId { get; set; }

    private ProductImage(ImageId id, ProductId productId) : base(id)
    {
        ProductId = productId;
    }
    
    public static ProductImage Create(ImageId id, ProductId productId) => new (id, productId);
}