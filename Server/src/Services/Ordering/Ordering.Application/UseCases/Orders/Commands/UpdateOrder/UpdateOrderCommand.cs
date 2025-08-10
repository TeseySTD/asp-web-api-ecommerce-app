using Ordering.Application.Dto.Order;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(Guid CustomerId, Guid OrderId, OrderUpdateDto Value) : ICommand;