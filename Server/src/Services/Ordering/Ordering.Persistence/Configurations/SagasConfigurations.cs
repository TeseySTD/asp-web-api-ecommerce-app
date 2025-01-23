using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ordering.Application.UseCases.Orders.Sagas;
using Shared.Messaging.Events.Order;

namespace Ordering.Persistence.Configurations;

public class MakeOrderSagaStateConfiguration : IEntityTypeConfiguration<MakeOrderSagaState>
{
    public void Configure(EntityTypeBuilder<MakeOrderSagaState> builder)
    {
        builder.HasKey(saga => saga.CorrelationId);
        ConfigureProductWithQuantityDto(builder);
    }

    private void ConfigureProductWithQuantityDto(EntityTypeBuilder<MakeOrderSagaState> builder)
    {
        builder.Property(x => x.ProductWithQuantityDtos)
            .HasConversion(
                v => JsonSerializer.Serialize(v, new JsonSerializerOptions{}),
                v => JsonSerializer.Deserialize<List<ProductWithQuantityDto>>(v, new JsonSerializerOptions{})
            );
    }
}