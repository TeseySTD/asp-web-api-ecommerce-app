using EcommerceProject.Application.Dto.Order;

namespace EcommerceProject.API.Http.Order.Responses;

public record GetOrdersResponse(OrderReadDto Value);