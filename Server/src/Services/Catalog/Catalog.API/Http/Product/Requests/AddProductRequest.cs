using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Http.Product.Requests;

public record AddProductRequest(
    string Title,
    string Description,
    decimal Price,
    uint Quantity,
    Guid CategoryId
);