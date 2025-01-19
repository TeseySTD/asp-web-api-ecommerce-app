namespace Shared.Messaging.Messages;

public record IntegrationMessage
{
    public Guid Id => Guid.NewGuid();
    public DateTime CreatedOnUtc => DateTime.UtcNow;
    public string MessageType => GetType().Name;
}