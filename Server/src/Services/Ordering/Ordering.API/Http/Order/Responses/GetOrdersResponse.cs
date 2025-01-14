using Ordering.Application.Dto.Order;

namespace Ordering.API.Http.Order.Responses;

public record GetOrdersResponse(OrderReadDto Value);