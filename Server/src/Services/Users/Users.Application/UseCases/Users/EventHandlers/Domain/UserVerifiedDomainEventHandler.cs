using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Domain.Classes;
using Users.Core.Models.Events;

namespace Users.Application.UseCases.Users.EventHandlers.Domain;

public class UserEmailVerifiedDomainEventHandler : INotificationHandler<UserEmailVerifiedDomainEvent>
{
    private readonly ILogger<UserEmailVerifiedDomainEventHandler> _logger;

    public UserEmailVerifiedDomainEventHandler(ILogger<UserEmailVerifiedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(UserEmailVerifiedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            notification.EventType, notification.OccurredOnUtc,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}