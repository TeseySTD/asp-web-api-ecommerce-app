namespace Basket.API.Http.Cart.Responses;

public record ProductCartItemResponse(
    Guid Id,
    string Title,
    uint StockQuantity,
    decimal Price,
    string[] ImageUrls,
    ProductCartItemCategoryResponse Category
);

public record ProductCartItemCategoryResponse(Guid Id, string Name);