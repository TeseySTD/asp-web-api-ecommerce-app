using System.Text.Json;
using Catalog.Core.Models.Categories.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Application.UseCases.Categories.EventHandlers;

public class CategoryUpdatedDomainEventHandler : INotificationHandler<CategoryUpdatedDomainEvent>
{
    private readonly ILogger<CategoryUpdatedDomainEventHandler> _logger;

    public CategoryUpdatedDomainEventHandler(ILogger<CategoryUpdatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CategoryUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification as DomainEvent;
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            domainEvent.EventType, domainEvent.OccurredOnUtc,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}