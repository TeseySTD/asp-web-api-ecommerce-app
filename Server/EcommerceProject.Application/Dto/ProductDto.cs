namespace EcommerceProject.Application.Dto;

public record ProductDto(Guid Id, string Title, string Description, decimal Price, uint Quantity);