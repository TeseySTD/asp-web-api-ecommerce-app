using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Order;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Application.UseCases.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, OrderReadDto>
{
    private readonly IOrdersRepository _ordersRepository;

    public GetOrderByIdQueryHandler(IOrdersRepository ordersRepository)
    {
        _ordersRepository = ordersRepository;
    }

    public async Task<Result<OrderReadDto>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _ordersRepository.FindById(request.OrderId, cancellationToken);
        if (order == null)
            return new Error("No orders found", "There are no orders in the database.");


        var products = order.OrderItems.Select(oi =>
            new OrderReadItemDto(
                ProductId: oi.Product.Id.Value,
                ProductTitle: oi.Product.Title.Value,
                ProductDescription: oi.Product.Description.Value
            )
        );

        var orderDto = new OrderReadDto(
            OrderId: order.Id.Value,
            UserName: order.User.Name.Value,
            Email: order.User.Email.Value,
            OrderDate: order.OrderDate.ToString(),
            Status: order.Status.ToString(),
            CardName: order.Payment.CardName,
            ShortCardNumber: order.Payment.CardNumber,
            Address: order.DestinationAddress.AddressLine,
            Products: products,
            TotalPrice: order.TotalPrice
        );

        return orderDto;
    }
}