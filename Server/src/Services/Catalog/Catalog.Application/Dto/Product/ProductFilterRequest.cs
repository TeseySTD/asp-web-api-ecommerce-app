namespace Catalog.Application.Dto.Product;

public record ProductFilterRequest(
    string? Title = null,
    Guid? CategoryId = null,
    int? MinPrice = null,
    int? MaxPrice = null
);