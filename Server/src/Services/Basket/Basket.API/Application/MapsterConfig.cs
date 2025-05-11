using Basket.API.Dto.Cart;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Mapster;

namespace Basket.API.Application;

public static class MapsterConfig
{
    public static void Configure(IServiceProvider serviceProvider)
    {
        ConfigureProductCart();
        ConfigureProductCartItem();
        ConfigureProductCartItemCategory();
    }

    public static void ConfigureProductCart()
    {
        TypeAdapterConfig<UserId, Guid>.NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<Guid, UserId>.NewConfig()
            .MapWith(src => UserId.Create(src).Value);

        TypeAdapterConfig<ProductCartDto, ProductCart>.NewConfig()
            .MapWith(src => src == null
                ? null
                : 
                ProductCart.Create(
                    src.UserId.Adapt<UserId>(),
                    src.Items.Adapt<ProductCartItem[]>()
                )
            );
        
        TypeAdapterConfig<ProductCart, ProductCartDto>.NewConfig()
            .MapWith(src => src == null
                ? null
                : new ProductCartDto(
                    src.Id.Adapt<Guid>(),
                    src.Items.Adapt<ProductCartItemDto[]>()
                )
            );
    }

    public static void ConfigureProductCartItem()
    {
        TypeAdapterConfig<Guid, ProductId>.NewConfig()
            .MapWith(src => ProductId.Create(src).Value);
        TypeAdapterConfig<string, ProductTitle>.NewConfig()
            .MapWith(src => ProductTitle.Create(src).Value);
        TypeAdapterConfig<decimal, ProductPrice>.NewConfig()
            .MapWith(src => ProductPrice.Create(src).Value);
        TypeAdapterConfig<uint, StockQuantity>.NewConfig()
            .MapWith(src => StockQuantity.Create(src).Value);

        TypeAdapterConfig<ProductId, Guid>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<ProductTitle, string>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<ProductPrice, decimal>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<StockQuantity, uint>.NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<ProductCartItemDto, ProductCartItem>.NewConfig()
            .MapWith(src => src == null
                ? null
                : ProductCartItem.Create(
                    src.Id.Adapt<ProductId>(),
                    src.Title.Adapt<ProductTitle>(),
                    src.StockQuantity.Adapt<StockQuantity>(),
                    src.Price.Adapt<ProductPrice>(),
                    src.Category.Adapt<ProductCartItemCategory>(),
                    src.ImageUrls
                )
            );

        TypeAdapterConfig<ProductCartItem, ProductCartItemDto>.NewConfig()
            .MapWith(src => src == null
                ? null
                : new ProductCartItemDto(
                    src.Id.Adapt<Guid>(),
                    src.Title.Adapt<string>(),
                    src.StockQuantity.Adapt<uint>(),
                    src.Price.Adapt<decimal>(),
                    src.ImageUrls.ToArray(),
                    src.Category.Adapt<ProductCartItemCategoryDto>()
                )
            );
    }

    public static void ConfigureProductCartItemCategory()
    {
        TypeAdapterConfig<CategoryId, Guid>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<CategoryName, string>.NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<Guid, CategoryId>.NewConfig()
            .MapWith(src => CategoryId.Create(src).Value);
        TypeAdapterConfig<string, CategoryName>.NewConfig()
            .MapWith(src => CategoryName.Create(src).Value);

        TypeAdapterConfig<ProductCartItemCategoryDto, ProductCartItemCategory>.NewConfig()
            .MapWith(src => src == null
                ? null
                : ProductCartItemCategory.Create(
                    src.Id.Adapt<CategoryId>(),
                    src.Name.Adapt<CategoryName>()
                )
            );

        TypeAdapterConfig<ProductCartItemCategory, ProductCartItemCategoryDto>.NewConfig()
            .MapWith(src => src == null
                ? null
                : new ProductCartItemCategoryDto(
                    src.Id.Adapt<Guid>(),
                    src.CategoryName.Adapt<string>()
                )
            );
    }
}