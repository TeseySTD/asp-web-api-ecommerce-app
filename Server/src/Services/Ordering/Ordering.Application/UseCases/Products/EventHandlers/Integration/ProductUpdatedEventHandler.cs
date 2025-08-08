using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Ordering.Application.UseCases.Products.EventHandlers.Integration;

public class ProductUpdatedEventHandler : IntegrationEventHandler<ProductUpdatedEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public ProductUpdatedEventHandler(ILogger<IntegrationEventHandler<ProductUpdatedEvent>> logger,
        IApplicationDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    public override async Task Handle(ConsumeContext<ProductUpdatedEvent> context)
    {
        var productToUpdateId = ProductId.Create(context.Message.ProductId).Value;
        var productToUpdate = await _dbContext.Products.FindAsync(productToUpdateId);

        productToUpdate!.Update(
            title: ProductTitle.Create(context.Message.Title).Value,
            description: ProductDescription.Create(context.Message.Description).Value);

        await _dbContext.SaveChangesAsync(default);
    }
}