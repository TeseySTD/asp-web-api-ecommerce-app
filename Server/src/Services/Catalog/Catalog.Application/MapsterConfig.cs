using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Application.Dto.Product;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.Entities;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.Entities;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Catalog.Application;

public static class MapsterConfig
{
    public static void Configure(IServiceProvider serviceProvider)
    {
        var imageUrlGenerator = serviceProvider.GetRequiredService<IImageUrlGenerator>();
        ConfigureCategory(imageUrlGenerator);
        ConfigureProduct(imageUrlGenerator);
    }

    private static void ConfigureCategory(IImageUrlGenerator imageUrlGenerator)
    {
        TypeAdapterConfig<CategoryId, Guid>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<CategoryName, string>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<CategoryDescription, string>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<CategoryImage, string>.NewConfig()
            .MapWith(src => imageUrlGenerator.GenerateUrl(src.Id));

        TypeAdapterConfig<Category, CategoryReadDto>.NewConfig()
            .MapWith(src => src == null
                ? null
                : new CategoryReadDto
                (
                    src.Id.Value,
                    src.Name.Value,
                    src.Description.Value,
                    src.Images.Adapt<string[]>()
                )
            );
    }

    private static void ConfigureProduct(IImageUrlGenerator imageUrlGenerator)
    {
        TypeAdapterConfig<ProductId, Guid>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<ProductDescription, string>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<ProductTitle, string>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<ProductPrice, decimal>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<StockQuantity, uint>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<SellerId, Guid>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<ProductImage, string>.NewConfig()
            .MapWith(src => imageUrlGenerator.GenerateUrl(src.Id));

        TypeAdapterConfig<Product, ProductReadDto>.NewConfig()
            .MapWith(src => src == null
                ? null
                : new ProductReadDto
                (
                    src.Id.Value,
                    src.Title.Value,
                    src.Description.Value,
                    src.Price.Value,
                    src.StockQuantity.Value,
                    src.SellerId.Value,
                    src.Images.Adapt<string[]>(),
                    src.Category.Adapt<CategoryReadDto>()
                )
            );
    }
}