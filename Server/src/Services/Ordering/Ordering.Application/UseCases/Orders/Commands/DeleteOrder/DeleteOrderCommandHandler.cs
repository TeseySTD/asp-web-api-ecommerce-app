using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Core.Models.Orders;
using Shared.Core.CQRS;
using Shared.Core.Validation;

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
        var result = Result.TryFail()
            .CheckError(!await _context.Orders.AnyAsync(o => o.Id == request.OrderId, cancellationToken),
                new Error(nameof(Order), $"Order with id: {request.OrderId.Value} does not exist"))
            .Build();

        if (result.IsFailure)
            return result;

        await _context.Orders.Where(o => o.Id == request.OrderId).ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
}