using System.Text.Json;
using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Users.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcommerceProject.Application.UseCases.Users.EventHandlers;

public class UserUpdatedDomainEventHandler : INotificationHandler<UserUpdatedDomainEvent>
{
    private readonly ILogger<UserUpdatedDomainEventHandler> _logger;

    public UserUpdatedDomainEventHandler(ILogger<UserUpdatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification as IDomainEvent;
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            domainEvent.EventType, domainEvent.OccurredOn,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}