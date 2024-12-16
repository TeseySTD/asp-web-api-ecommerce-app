using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Orders.ValueObjects;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.Dto.Order;

public record OrderWriteDto(
    Guid UserId,
    IEnumerable<(Guid ProductId, uint Quantity, uint Price)> OrderItems,
    (string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod) Payment,
    (string addressLine, string? country, string? state, string? zipCode) DestinationAddress
);