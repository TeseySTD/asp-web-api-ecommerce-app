using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products.ValueObjects;

namespace Ordering.Persistence.Configurations;

public class OrderConfigurations : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        ConfigureOrderTable(builder);
    }

    private void ConfigureOrderTable(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => OrderId.Create(value).Value);
        
        builder.HasOne(o => o.Customer)
            .WithMany()
            .HasForeignKey(o => o.CustomerId)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey(o => o.OrderId)
            .HasPrincipalKey(o => o.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(o => o.OrderDate);

        builder.ComplexProperty(o => o.Payment, pb =>
        {
            pb.Property(p => p.CardName)
                .HasMaxLength(Payment.MaxCardNameLength);

            pb.Property(p => p.CardNumber)
                .HasMaxLength(Payment.MaxCardNumberLength);

            pb.Property(p => p.Expiration)
                .HasMaxLength(Payment.MaxExpirationLength);

            pb.Property(p => p.CVV)
                .HasMaxLength(Payment.CVVLength);

            pb.Property(p => p.PaymentMethod);
        });

        builder.ComplexProperty(o => o.DestinationAddress, ab =>
        {
            ab.Property(a => a.AddressLine);

            ab.Property(a => a.Country);

            ab.Property(a => a.State);

            ab.Property(a => a.ZipCode);
        });

        builder.Property(x => x.Status)
            .HasConversion(
                status => status.ToString(),
                value => (OrderStatus)Enum.Parse(typeof(OrderStatus), value));
    }
}

public class OrderItemConfigurations : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id)
            .HasConversion(
                id => id.Value,
                value => OrderItemId.Create(value).Value);
        
        
        builder.HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .HasPrincipalKey(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);


        builder.Property(o => o.Quantity)
            .HasConversion(
                q => q.Value,
                value => OrderItemQuantity.Create(value).Value);

        builder.Property(o => o.Price)
            .HasConversion(
                price => price.Value,
                value => ProductPrice.Create(value).Value);
    }
}