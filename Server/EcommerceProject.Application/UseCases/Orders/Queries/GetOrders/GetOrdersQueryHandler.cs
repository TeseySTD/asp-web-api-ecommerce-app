using System.Globalization;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Order;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IEnumerable<OrderReadDto>>
{
    private readonly IOrdersRepository _ordersRepository;

    public GetOrdersQueryHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<Result<IEnumerable<OrderReadDto>>> Handle(GetOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders = await _ordersRepository.GetAll(cancellationToken);
        if (!orders.Any())
            return new Error("No orders found", "There are no orders in the database.");

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