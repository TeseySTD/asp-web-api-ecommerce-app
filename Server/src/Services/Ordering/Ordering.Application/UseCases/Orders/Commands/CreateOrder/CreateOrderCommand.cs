using Ordering.Application.Dto.Order;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Commands.CreateOrder;

public record CreateOrderCommand(OrderWriteDto Value) : ICommand<Guid>;