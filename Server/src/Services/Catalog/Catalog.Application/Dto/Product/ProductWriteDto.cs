using Catalog.Application.Dto.Image;

namespace Catalog.Application.Dto.Product;

public record ProductWriteDto(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    uint Quantity,
    Guid CategoryId);