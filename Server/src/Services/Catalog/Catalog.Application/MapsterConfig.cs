using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using Mapster;

namespace Catalog.Application;

public static class MapsterConfig
{
    public static void Configure()
    {
        ConfigureCategory();
        ConfigureProduct();
    }

    private static void ConfigureCategory()
    {
        TypeAdapterConfig<CategoryId, Guid>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<CategoryName, string>.NewConfig()
            .MapWith(src => src.Value);
        TypeAdapterConfig<CategoryDescription, string>.NewConfig()
            .MapWith(src => src.Value);

        TypeAdapterConfig<Category, CategoryDto>.NewConfig()
            .MapWith(src => src == null
                ? null : new CategoryDto
                (
                    src.Id.Value,
                    src.Name.Value,
                    src.Description.Value
                )
            );
    }

    private static void ConfigureProduct()
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
    }
}