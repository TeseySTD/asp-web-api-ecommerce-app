namespace Catalog.API.Http.Product.Requests;

public record GetProductsRequest(
    string? Title = null,
    Guid? CategoryId = null,
    int? MinPrice = null,
    int? MaxPrice = null
);