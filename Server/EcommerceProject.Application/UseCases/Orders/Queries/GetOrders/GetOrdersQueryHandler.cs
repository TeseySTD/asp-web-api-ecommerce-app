using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Order;
using EcommerceProject.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Application.UseCases.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IEnumerable<OrderReadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetOrdersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<OrderReadDto>>> Handle(GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        if (!await _context.Orders.AnyAsync())
            return new Error("No orders found", "There are no orders in the database.");

        var orders = await _context.Orders
            .AsNoTracking()
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .ToListAsync(cancellationToken);

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
                UserName: o.User.Name.Value,
                Email: o.User.Email.Value,
                OrderDate: o.OrderDate.ToString(),
                Status: o.Status.ToString(),
                CardName: o.Payment.CardName,
                ShortCardNumber: o.Payment.CardNumber,
                Address: o.DestinationAddress.AddressLine,
                Products: products,
                TotalPrice: o.TotalPrice
            );
        }).ToList();

        return orderDtos;
    }
}