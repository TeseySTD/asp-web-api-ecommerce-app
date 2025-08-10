using MassTransit;
using Microsoft.Extensions.Logging;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;

namespace Ordering.Application.UseCases.Orders.EventHandlers.Integration;

public class CanceledOrderEventHandler : IntegrationEventHandler<CanceledOrderEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public CanceledOrderEventHandler(ILogger<IntegrationEventHandler<CanceledOrderEvent>> logger,
        IApplicationDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    public override async Task Handle(ConsumeContext<CanceledOrderEvent> context)
    {
        Logger.Log(LogLevel.Warning, "Order with id {Id} cancelled by reason {Reason}"
            , context.Message.OrderId, context.Message.Reason);

        var orderId = OrderId.Create(context.Message.OrderId).Value;
        var order = await _dbContext.Orders.FindAsync(orderId);

        order!.Cancel();
        await _dbContext.SaveChangesAsync(default);
    }
}