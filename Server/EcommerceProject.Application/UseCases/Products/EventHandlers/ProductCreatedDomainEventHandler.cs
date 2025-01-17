using System.Text.Json;
using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Products.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EcommerceProject.Application.UseCases.Products.EventHandlers;

public class ProductCreatedDomainEventHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;

    public ProductCreatedDomainEventHandler(ILogger<ProductCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification as IDomainEvent;
        _logger.LogInformation("Domain event {Type} on {Time} handled: {DomainEvent}",
            domainEvent.EventType, domainEvent.OccurredOn,
            JsonSerializer.Serialize(notification, new JsonSerializerOptions { WriteIndented = true, IncludeFields = true}));
        return Task.CompletedTask;
    }
}
