namespace Basket.API.Dto.Cart;

public record ProductCartItemDto(
    Guid Id,
    string Title,
    uint StockQuantity,
    decimal Price,
    string[] ImageUrls,
    ProductCartItemCategoryDto Category
);

public record ProductCartItemCategoryDto(Guid Id, string Name);