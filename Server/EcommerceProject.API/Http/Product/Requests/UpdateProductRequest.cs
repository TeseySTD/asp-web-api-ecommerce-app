namespace EcommerceProject.API.Http.Product.Requests;

public record UpdateProductRequest(string Title, string Description, decimal Price, uint Quantity, Guid CategoryId);