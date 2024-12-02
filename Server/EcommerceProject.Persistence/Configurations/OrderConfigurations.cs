using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Orders.Entities;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcommerceProject.Persistence.Configurations;

public class OrderConfigurations : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        ConfigureOrderTable(builder);
        ConfigureOrderItemTable(builder);
    }
    
    private void ConfigureOrderTable(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .ValueGeneratedNever()
            .HasConversion(
                id => id.Value,
                value => OrderId.Create(value));
        
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.Property(o => o.UserId)
            .ValueGeneratedNever()
            .HasConversion(
                userId => userId.Value,
                value => UserId.Create(value));
        
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
            .HasDefaultValue(OrderStatus.NotStarted)
            .HasConversion(
                status => status.ToString(),
                value => (OrderStatus)Enum.Parse(typeof(OrderStatus), value))
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore); // Sentinel value
    }
    
    private void ConfigureOrderItemTable(EntityTypeBuilder<Order> builder)
    {
        builder.OwnsMany(o => o.OrderItems, oib =>
        {
            oib.ToTable("OrderItems");

            oib.HasKey(x => x.Id);

            oib.Property(o => o.Id)
                .HasConversion(
                    id => id.Value,
                    value => OrderItemId.Create(value));

            oib.Property(o => o.ProductId)
                .HasConversion(
                    id => id.Value,
                    value => ProductId.Create(value));

            oib.HasOne<Product>()
                .WithMany()
                .HasForeignKey("ProductId")
                .OnDelete(DeleteBehavior.Cascade);

            oib.WithOwner()
                .HasForeignKey("OrderId");

            oib.Property(o => o.Quantity)
                .HasConversion(
                    q => q.Value,
                    value => OrderItemQuantity.Create(value));

            oib.Property(o => o.Price)
                .HasConversion(
                    price => price.Value,
                    value => ProductPrice.Create(value));
        });
    }
}