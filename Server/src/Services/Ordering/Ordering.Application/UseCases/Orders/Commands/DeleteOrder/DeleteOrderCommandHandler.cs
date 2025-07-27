using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Application.UseCases.Orders.Commands.DeleteOrder;

public class DeleteOrderCommandHandler : ICommandHandler<DeleteOrderCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var result = Result.Try()
            .Check(!await _context.Orders.AnyAsync(o => o.Id == request.OrderId, cancellationToken),
                new OrderNotFoundError(request.OrderId.Value))
            .Build();

        if (result.IsFailure)
            return result;

        await _context.Orders.Where(o => o.Id == request.OrderId).ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
    
    public sealed record OrderNotFoundError(Guid OrderId) : Error("Order not found.", $"Order with id: {OrderId} does not exist");
}