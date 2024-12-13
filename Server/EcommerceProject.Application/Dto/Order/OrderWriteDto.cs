using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Orders.ValueObjects;

namespace EcommerceProject.Application.Dto.Order;

public record OrderWriteDto((IEnumerable<Guid>Products, IEnumerable<uint>Quantitys ) OrderItems,
    Payment Payment, Address DestinationAddress);