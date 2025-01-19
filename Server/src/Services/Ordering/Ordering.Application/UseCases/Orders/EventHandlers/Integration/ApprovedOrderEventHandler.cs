using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;

namespace Ordering.Application.UseCases.Orders.EventHandlers.Integration;

public class ApprovedOrderEventHandler : IntegrationEventHandler<ApprovedOrderEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public ApprovedOrderEventHandler(ILogger<IntegrationEventHandler<ApprovedOrderEvent>> logger,
        IApplicationDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }


    public override async Task Handle(ConsumeContext<ApprovedOrderEvent> context)
    {
        var orderId = OrderId.Create(context.Message.OrderId).Value;
        var order = await _dbContext.Orders.FindAsync(orderId);
        
        order!.Approve();

        await _dbContext.SaveChangesAsync(default);
    }
}