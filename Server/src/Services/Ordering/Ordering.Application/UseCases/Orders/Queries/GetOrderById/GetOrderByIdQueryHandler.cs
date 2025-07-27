using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Dto.Order;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Ordering.Application.UseCases.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderReadDto>
{
    private readonly IApplicationDbContext _context;

    public GetOrderByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrderReadDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        if (!await _context.Orders.AnyAsync(o => o.Id == request.OrderId, cancellationToken))
            return new OrderNotFoundError(request.OrderId.Value);

        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        var products = order!.OrderItems.Select(oi =>
            new OrderReadItemDto(
                ProductId: oi.Product.Id.Value,
                ProductTitle: oi.Product.Title.Value,
                ProductDescription: oi.Product.Description.Value
            )
        );

        var orderDto = new OrderReadDto(
            OrderId: order.Id.Value,
            CustomerId: order.CustomerId.Value,
            OrderDate: order.OrderDate.ToString(),
            Status: order.Status.ToString(),
            CardName: order.Payment.CardName,
            ShortCardNumber: order.Payment.CardNumber.Substring(0, 3),
            Address: order.DestinationAddress.AddressLine,
            Products: products,
            TotalPrice: order.TotalPrice
        );

        return orderDto;
    }

    public sealed record OrderNotFoundError(Guid OrderId) : Error("No order found", $"There are no order with id '{OrderId}' in the database.");
}