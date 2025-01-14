using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Core.Models.Customers;
using Ordering.Core.Models.Customers.ValueObjects;

namespace Ordering.Persistence.Configurations;

public class CustomerConfigurations : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        ConfigureUserTable(builder);
    }

    private void ConfigureUserTable(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value  => CustomerId.Create(value).Value);

        builder.Property(u => u.Name)
            .HasConversion(
                n => n.Value,
                value => CustomerName.Create(value).Value)
            .HasMaxLength(CustomerName.MaxNameLength);

        builder.Property(u => u.Email)
            .HasConversion(
                e => e.Value,
                value => Email.Create(value).Value);
        
        builder.Property(u => u.PhoneNumber)
            .HasConversion(
                p => p.Value,
                value => PhoneNumber.Create(value).Value);
    }
}