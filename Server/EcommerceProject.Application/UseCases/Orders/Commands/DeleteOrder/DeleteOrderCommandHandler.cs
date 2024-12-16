using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand>
{
    private readonly IOrdersRepository _ordersRepository;

    public DeleteOrderCommandHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        return await _ordersRepository.Delete(request.OrderId, cancellationToken);
    }
}