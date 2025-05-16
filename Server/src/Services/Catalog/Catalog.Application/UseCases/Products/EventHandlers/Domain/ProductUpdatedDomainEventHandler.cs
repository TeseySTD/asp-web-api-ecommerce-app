using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Products.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Interfaces;
using Shared.Messaging.Events.Product;

namespace Catalog.Application.UseCases.Products.EventHandlers.Domain;

public class ProductUpdatedDomainEventHandler : INotificationHandler<ProductUpdatedDomainEvent>
{
    private readonly ILogger<ProductUpdatedDomainEventHandler> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IImageUrlGenerator _imageUrlGenerator;

    public ProductUpdatedDomainEventHandler(
        ILogger<ProductUpdatedDomainEventHandler> logger,
        IPublishEndpoint publishEndpoint,
        IImageUrlGenerator imageUrlGenerator)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
        _imageUrlGenerator = imageUrlGenerator;
    }

    public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            notification.EventType, notification.OccurredOnUtc,
            JsonSerializer.Serialize(notification,
                new JsonSerializerOptions { WriteIndented = true, IncludeFields = true }));

        var product = notification.Product;

        await _publishEndpoint.Publish(new ProductUpdatedEvent(
            ProductId: product.Id.Value,
            Title: product.Title.Value,
            Description: product.Description.Value,
            Price: product.Price.Value,
            Category: product.Category == null ? null : new(product.CategoryId!.Value, product.Category!.Name.Value),
            ImageUrls: product.Images.Select(i => _imageUrlGenerator.GenerateUrl(i.Id)).ToArray()
        ));
    }
}