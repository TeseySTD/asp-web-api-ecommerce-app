using MassTransit;
using Microsoft.Extensions.Logging;

namespace Shared.Messaging.Events;

public abstract class IntegrationEventHandler<TEvent> : IConsumer<TEvent> where TEvent : IntegrationEvent
{
    protected readonly ILogger<IntegrationEventHandler<TEvent>> Logger;

    protected IntegrationEventHandler(ILogger<IntegrationEventHandler<TEvent>> logger)
    {
        Logger = logger;
    }

    public abstract Task Handle(ConsumeContext<TEvent> context);

    public async Task Consume(ConsumeContext<TEvent> context)
    {
        LogStartupMessage(context);

        try
        {
            await Handle(context);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling integration event {EventType}", context.Message.EventType);
            throw;
        }

        LogFinalMessage(context);
    }

    public void LogStartupMessage(ConsumeContext<TEvent> context) => Logger.LogInformation(
        "[START] Integration event {EventType} that occurred on {OccurredTime}, is handling...",
        context.Message.EventType, context.Message.OccuredOnUtc);

    public void LogFinalMessage(ConsumeContext<TEvent> context) => Logger.LogInformation(
        "[END] Integration event {EventType} that occurred on {OccurredTime}, handled at {Time}.",
        context.Message.EventType, context.Message.OccuredOnUtc, DateTime.UtcNow);
}
