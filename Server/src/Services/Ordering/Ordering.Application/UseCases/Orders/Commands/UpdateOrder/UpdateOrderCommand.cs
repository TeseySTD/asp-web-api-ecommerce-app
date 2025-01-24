using Ordering.Application.Dto.Order;
using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(Guid CustomerId, Guid OrderId, OrderUpdateDto Value) : ICommand;