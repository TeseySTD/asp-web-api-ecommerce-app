using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Configurations;

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
        
        builder.HasMany(x => x.Images)
            .WithOne()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}