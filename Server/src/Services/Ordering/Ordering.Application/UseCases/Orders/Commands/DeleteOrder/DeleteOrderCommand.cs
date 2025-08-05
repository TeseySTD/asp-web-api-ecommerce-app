using Ordering.Core.Models.Orders.ValueObjects;
using Shared.Core.Auth;
using Shared.Core.CQRS;

namespace Ordering.Application.UseCases.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(OrderId OrderId, CustomerId CustomerId, UserRole CustomerRole) : ICommand;