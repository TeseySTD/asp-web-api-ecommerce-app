using MassTransit;
using Microsoft.Extensions.Logging;
using Shared.Messaging.Events;

namespace Shared.Messaging.Messages;

public abstract class IntegrationMessageHandler<TMessage>: IConsumer<TMessage> where TMessage : IntegrationMessage
{
    protected readonly ILogger<IntegrationMessageHandler<TMessage>> Logger;

    protected IntegrationMessageHandler(ILogger<IntegrationMessageHandler<TMessage>> logger)
    {
        Logger = logger;
    }

    public abstract Task Handle(ConsumeContext<TMessage> context);

    public async Task Consume(ConsumeContext<TMessage> context)
    {
        LogStartupMessage(context);

        try
        {
            await Handle(context);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error while handling integration message {EventType}", context.Message.MessageType);
            throw;
        }

        LogFinalMessage(context);
    }

    protected void LogStartupMessage(ConsumeContext<TMessage> context) => Logger.LogInformation(
        "[START] Integration message {MessageType} that created on {CreatedTime}, is handling...",
        context.Message.MessageType, context.Message.CreatedOnUtc);

    protected void LogFinalMessage(ConsumeContext<TMessage> context) => Logger.LogInformation(
        "[END] Integration message {MessageType} that created on {CreatedTime}, handled at {Time}.",
        context.Message.MessageType, context.Message.CreatedOnUtc, DateTime.UtcNow);
    
}