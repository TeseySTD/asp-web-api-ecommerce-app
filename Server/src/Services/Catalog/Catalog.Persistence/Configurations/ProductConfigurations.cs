using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations;

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

        builder
            .Property(p => p.StockQuantity)
            .HasConversion(
                p => p.Value,
                value => StockQuantity.Create(value));
        
        builder.Property(p => p.CategoryId)
            .HasConversion(
                c => c.Value,
                value => CategoryId.Create(value).Value);

        builder.HasOne(p => p.Category)
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .HasPrincipalKey(c => c.Id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}