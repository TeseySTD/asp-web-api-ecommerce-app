using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(OrderId OrderId) : ICommand;