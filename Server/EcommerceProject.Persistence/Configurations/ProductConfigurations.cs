using System;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceProject.Persistence.Configurations;

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

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .HasPrincipalKey(c => c.Id)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Id)
            .HasConversion(
                c => c.Value,
                value => CategoryId.Create(value).Value);
        
        builder
            .Property(p => p.Name)
            .HasConversion(
                c => c.Value,
                value => CategoryName.Create(value).Value)
            .HasMaxLength(CategoryName.MaxNameLength);

        builder
            .Property(p => p.Description)
            .HasConversion(
                c => c.Value,
                value => CategoryDescription.Create(value).Value)
            .HasMaxLength(CategoryDescription.MaxDescriptionLength);
    }
}