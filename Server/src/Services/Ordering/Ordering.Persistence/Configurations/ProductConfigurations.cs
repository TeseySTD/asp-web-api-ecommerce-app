using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;

namespace Ordering.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder
            .Property(p => p.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value).Value);

        builder
            .Property(p => p.Title)
            .HasMaxLength(ProductTitle.MaxTitleLength)
            .HasConversion(
                t => t.Value,
                value => ProductTitle.Create(value).Value);

        builder
            .Property(p => p.Description)
            .HasMaxLength(ProductDescription.MaxDescriptionLength)
            .HasConversion(
                d => d.Value,
                value => ProductDescription.Create(value).Value);

        builder
            .Property(p => p.Price)
            .HasConversion(
                p => p.Value,
                value => ProductPrice.Create(value).Value);
    }
}
