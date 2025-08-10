namespace Catalog.Application.Dto.Product;

public record ProductUpdateDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    uint Quantity,
    Guid SellerId,
    Guid? CategoryId);