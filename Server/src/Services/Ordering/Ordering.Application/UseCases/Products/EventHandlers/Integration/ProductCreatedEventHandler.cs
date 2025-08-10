using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Ordering.Application.UseCases.Products.EventHandlers.Integration;

public class ProductCreatedEventHandler : IntegrationEventHandler<ProductCreatedEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public ProductCreatedEventHandler(ILogger<IntegrationEventHandler<ProductCreatedEvent>> logger,
        IApplicationDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    public override async Task Handle(ConsumeContext<ProductCreatedEvent> context)
    {
        var product = Product.Create(
            id: ProductId.Create(context.Message.ProductId).Value,
            title: ProductTitle.Create( context.Message.Title ).Value,
            description: ProductDescription.Create(context.Message.Description ).Value
        );

        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync(default);
    }
}