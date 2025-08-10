using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Dto.Order;
using Shared.Core.API;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Ordering.Application.UseCases.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, PaginatedResult<OrderReadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedResult<OrderReadDto>>> Handle(GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var pageIndex = request.PaginationRequest.PageIndex;
        var pageSize = request.PaginationRequest.PageSize;

        var query = _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.CustomerId == request.CustomerId)
            .AsNoTracking();
        
        if(request.OrderStatus != null)
            query = query.Where(o => o.Status == request.OrderStatus);

        var orders = await query
            .Skip(pageSize * pageIndex)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (!orders.Any())
            return new OrdersNotFoundError();

        var orderDtos = orders.Select(o =>
        {
            var products = o.OrderItems.Select(oi =>
                new OrderReadItemDto(
                    ProductId: oi.Product.Id.Value,
                    ProductTitle: oi.Product.Title.Value,
                    ProductDescription: oi.Product.Description.Value
                )
            );

            return new OrderReadDto(
                OrderId: o.Id.Value,
                CustomerId: o.CustomerId.Value,
                OrderDate: o.OrderDate.ToString(CultureInfo.InvariantCulture),
                Status: o.Status.ToString(),
                CardName: o.Payment.CardName,
                ShortCardNumber: o.Payment.CardNumber.Substring(0, 3),
                Address: o.DestinationAddress.AddressLine,
                Products: products,
                TotalPrice: o.TotalPrice
            );
        }).ToList();

        return new PaginatedResult<OrderReadDto>(pageIndex, pageSize, orderDtos);
    }

    public sealed record OrdersNotFoundError() : Error("No orders found", "There are no orders in the database.");
}