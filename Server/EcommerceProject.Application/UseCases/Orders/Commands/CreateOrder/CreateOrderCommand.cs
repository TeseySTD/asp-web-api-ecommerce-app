using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Order;

namespace EcommerceProject.Application.UseCases.Orders.Commands.CreateOrder;

public record CreateOrderCommand(OrderWriteDto Value) : ICommand<Guid>;