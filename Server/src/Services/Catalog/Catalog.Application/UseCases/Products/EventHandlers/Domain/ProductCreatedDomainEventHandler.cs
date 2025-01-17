using System.Text.Json;
using Catalog.Core.Models.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Application.UseCases.Products.EventHandlers.Domain;

public class ProductCreatedDomainEventHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;

    public ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            notification.EventType, notification.OccurredOnUtc,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}
