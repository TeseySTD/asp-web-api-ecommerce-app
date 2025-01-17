namespace Ordering.Application.Dto.Order;

public record OrderReadDto(
    Guid OrderId,
    Guid CustomerId,
    string OrderDate,
    string Status,
    string CardName,
    string ShortCardNumber,
    string Address,
    IEnumerable<OrderReadItemDto> Products,
    decimal TotalPrice
);

public record OrderReadItemDto(Guid ProductId, string ProductTitle, string ProductDescription);