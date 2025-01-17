namespace Shared.Messaging.Events.Order;

public record MakeOrderEvent : IntegrationEvent
{
    public Guid OrderId { get; init; }
    public List<MakeOrderEventProduct> Products { get; init; }

    public MakeOrderEvent(Guid orderId, List<MakeOrderEventProduct> products)
    {
        OrderId = orderId;
        Products = products;
    }
}

public record MakeOrderEventProduct(Guid ProductId, decimal ProductPrice, uint ProductQuantity);