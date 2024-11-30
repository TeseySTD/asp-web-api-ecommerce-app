namespace EcommerceProject.API.Contracts;

public record AddProductRequest(string Title, string Description, decimal Price, uint Quantity);