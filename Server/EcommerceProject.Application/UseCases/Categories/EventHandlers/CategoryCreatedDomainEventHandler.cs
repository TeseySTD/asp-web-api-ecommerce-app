using System.Text.Json;
using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Categories.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcommerceProject.Application.UseCases.Categories.EventHandlers;

public class CategoryCreatedDomainEventHandler : INotificationHandler<CategoryCreatedDomainEvent>
{
    private ILogger<CategoryCreatedDomainEventHandler> _logger;

    public CategoryCreatedDomainEventHandler(ILogger<CategoryCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(CategoryCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification as IDomainEvent;
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            domainEvent.EventType, domainEvent.OccurredOn,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}