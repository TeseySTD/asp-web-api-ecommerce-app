using System.Text.Json;
using Catalog.Core.Models.Categories.Events;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Classes;
using Shared.Messaging.Events.Category;

namespace Catalog.Application.UseCases.Categories.EventHandlers;

public class CategoryCreatedDomainEventHandler : INotificationHandler<CategoryCreatedDomainEvent>
{
    private ILogger<CategoryCreatedDomainEventHandler> _logger;
    private IPublishEndpoint _publishEndpoint;

    public CategoryCreatedDomainEventHandler(ILogger<CategoryCreatedDomainEventHandler> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Handle(CategoryCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification as DomainEvent;
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            domainEvent.EventType, domainEvent.OccurredOnUtc,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        
        await _publishEndpoint.Publish<CategoryCreatedEvent>(
            new(notification.CategoryId.Value, notification.CategoryName.Value));
    }
}