using System.Text.Json;
using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcommerceProject.Application.UseCases.Orders.EventHandlers;

public class OrderUpdatedDomainEventHandler : INotificationHandler<OrderUpdatedDomainEvent>
{
    private readonly ILogger<OrderUpdatedDomainEventHandler> _logger;

    public OrderUpdatedDomainEventHandler(ILogger<OrderUpdatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification as IDomainEvent;
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            domainEvent.EventType, domainEvent.OccurredOn,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}