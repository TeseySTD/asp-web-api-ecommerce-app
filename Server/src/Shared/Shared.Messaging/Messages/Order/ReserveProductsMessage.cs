using Shared.Messaging.Events.Order;

namespace Shared.Messaging.Messages.Order;

public record ReserveProductsMessage(Guid OrderId, List<ProductWithQuantityDto> Products) : IntegrationMessage;

