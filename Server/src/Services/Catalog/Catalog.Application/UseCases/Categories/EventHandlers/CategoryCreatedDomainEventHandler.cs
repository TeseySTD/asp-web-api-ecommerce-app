using System.Text.Json;
using Catalog.Core.Models.Categories.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Classes;

namespace Catalog.Application.UseCases.Categories.EventHandlers;

public class CategoryCreatedDomainEventHandler : INotificationHandler<CategoryCreatedDomainEvent>
{
    private ILogger<CategoryCreatedDomainEventHandler> _logger;

    public CategoryCreatedDomainEventHandler(ILogger<CategoryCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CategoryCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification as DomainEvent;
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            domainEvent.EventType, domainEvent.OccurredOnUtc,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}