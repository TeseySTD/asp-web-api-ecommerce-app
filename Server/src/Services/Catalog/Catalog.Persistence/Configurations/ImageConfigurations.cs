using Catalog.Core.Models.Categories.Entities;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products.Entities;
using Catalog.Core.Models.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ImageId.Create(value).Value);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => FileName.Create(value).Value
            )
            .HasMaxLength(256);

        builder.Property(x => x.Data)
            .IsRequired()
            .HasConversion(
                value => value.Value,
                value => ImageData.Create(value).Value)
            .HasMaxLength(ImageData.MaxSize);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<ImageContentType>(value)
            );
    }
}

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ImageId.Create(value).Value);

        builder.Property(x => x.ProductId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => ProductId.Create(value).Value);

        builder.HasOne<Image>()
            .WithMany()
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CategoryImageConfiguration : IEntityTypeConfiguration<CategoryImage>
{
    public void Configure(EntityTypeBuilder<CategoryImage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => ImageId.Create(value).Value);

        builder.Property(x => x.CategoryId)
            .IsRequired()
            .HasConversion(
                id => id.Value,
                value => CategoryId.Create(value).Value);

        builder.HasOne<Image>()
            .WithMany()
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}