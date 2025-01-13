using Catalog.Application.Dto.Category;

namespace Catalog.Application.Dto.Product;

public record ProductReadDto(Guid Id, string Title, string Description, decimal Price, uint Quantity, CategoryDto? Category);