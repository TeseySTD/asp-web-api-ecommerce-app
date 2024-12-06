namespace EcommerceProject.Application.Dto.Product;

public record ProductDto(Guid Id, string Title, string Description, decimal Price, uint Quantity);