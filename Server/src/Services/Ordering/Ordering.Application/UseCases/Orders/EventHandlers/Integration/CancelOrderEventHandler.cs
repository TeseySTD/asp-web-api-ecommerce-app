using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;

namespace Ordering.Application.UseCases.Orders.EventHandlers.Integration;

public class CancelOrderEventHandler : IntegrationEventHandler<CancelOrderEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public CancelOrderEventHandler(ILogger<IntegrationEventHandler<CancelOrderEvent>> logger,
        IApplicationDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    public override async Task Handle(ConsumeContext<CancelOrderEvent> context)
    {
        Logger.Log(LogLevel.Warning, "Order with id {Id} cancelled by reason {Reason}"
            , context.Message.Id, context.Message.Reason);

        var orderId = OrderId.Create(context.Message.OrderId).Value;
        var order = await _dbContext.Orders.FindAsync(orderId);

        order!.CancelOrder();
        await _dbContext.SaveChangesAsync(default);
    }
}