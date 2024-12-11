namespace EcommerceProject.API.Http.Product.Requests;

public record UpdateProductRequest(Guid Id, string Title, string Description, decimal Price, uint Quantity, Guid CategoryId);