
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(OrderId OrderId) : ICommand;