using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events.Order;
using Shared.Messaging.Messages;
using Shared.Messaging.Messages.Order;
using Users.Application.Common.Interfaces;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Users.EventHandlers.Integration;

public class CheckCustomerMessageHandler : IntegrationMessageHandler<CheckCustomerMessage>
{
    private readonly IApplicationDbContext _dbContext;
    
    public CheckCustomerMessageHandler(IApplicationDbContext dbContext,
        ILogger<IntegrationMessageHandler<CheckCustomerMessage>> logger) : base(logger)
    {
        _dbContext = dbContext;
    }

    public override async Task Handle(ConsumeContext<CheckCustomerMessage> context)
    {
        var userId = UserId.Create(context.Message.CustomerId).Value;
        
        if (await _dbContext.Users.AnyAsync(x => x.Id == userId))
            await context.Publish(new CheckedCustomerEvent(context.Message.OrderId));
        else
            await context.Publish(new CheckingCustomerFailedEvent(context.Message.OrderId));
    }
}