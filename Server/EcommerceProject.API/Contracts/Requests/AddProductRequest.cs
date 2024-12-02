namespace EcommerceProject.API.Contracts.Requests;

public record AddProductRequest(string Title, string Description, decimal Price, uint Quantity);