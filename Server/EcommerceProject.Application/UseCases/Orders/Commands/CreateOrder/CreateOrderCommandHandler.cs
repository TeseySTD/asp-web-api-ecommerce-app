using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand, Guid>
{
    public Task<Result<Guid>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}