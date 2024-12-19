using EcommerceProject.API.Http;
using EcommerceProject.API.Http.Order.Requests;
using EcommerceProject.API.Http.Order.Responses;
using EcommerceProject.Application.Dto.Order;
using EcommerceProject.Application.UseCases.Orders.Commands.CreateOrder;
using EcommerceProject.Application.UseCases.Orders.Commands.DeleteOrder;
using EcommerceProject.Application.UseCases.Orders.Commands.UpdateOrder;
using EcommerceProject.Application.UseCases.Orders.Queries.GetOrderById;
using EcommerceProject.Application.UseCases.Orders.Queries.GetOrders;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/orders")]
public class OrderController : ApiController
{
    public OrderController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<ActionResult<GetOrdersResponse>> GetOrders()
    {
        var query = new GetOrdersQuery();
        var result = await Sender.Send(query);

        return result.Map<ActionResult<GetOrdersResponse>>(
            onSuccess: value => Ok(value),
            onFailure: errors => NotFound(Envelope.Of(errors)));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderReadDto>> GetOrder(Guid id)
    {
        var query = new GetOrderByIdQuery(OrderId.Create(id).Value);
        var result = await Sender.Send(query);

        return result.Map<ActionResult<OrderReadDto>>(
            onSuccess: value => Ok(value),
            onFailure: errors => NotFound(Envelope.Of(errors)));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> MakeOrder([FromBody] MakeOrderRequest request)
    {
        var payment = (
            request.cardName,
            request.cardNumber,
            request.expiration,
            request.cvv,
            request.paymentMethod
        );

        var address = (
            request.addressLine,
            request.country,
            request.state,
            request.zipCode
        );

        var orderItems = request.OrderItems.Select(i =>
            (
                i.ProductId,
                i.Quantity,
                i.Price
            )
        );

        var dto = new OrderWriteDto(
            UserId: request.UserId,
            OrderItems: orderItems,
            Payment: payment,
            DestinationAddress: address
        );

        var cmd = new CreateOrderCommand(dto);
        var result = await Sender.Send(cmd);

        return result.Map<ActionResult<Guid>>(
            onSuccess: value => Ok(value),
            onFailure: errors => BadRequest(Envelope.Of(errors))
        );
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] UpdateOrderRequest request)
    {
        var payment = (
            request.cardName,
            request.cardNumber,
            request.expiration,
            request.cvv,
            request.paymentMethod
        );

        var address = (
            request.addressLine,
            request.country,
            request.state,
            request.zipCode
        );

        var orderItems = request.OrderItems.Select(i =>
            (
                i.ProductId,
                i.Quantity,
                i.Price
            )
        );

        var dto = new OrderUpdateDto(
            OrderItems: orderItems,
            Payment: payment,
            DestinationAddress: address
        );
        
        var cmd = new UpdateOrderCommand(OrderId.Create(id).Value, dto);
        var result = await Sender.Send(cmd);

        
        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => BadRequest(Envelope.Of(errors)));

    }
    
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteOrder(Guid id)
    {
        var cmd = new DeleteOrderCommand(OrderId.Create(id).Value);
        var result = await Sender.Send(cmd);
        
        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => BadRequest(Envelope.Of(errors)));
    }
}