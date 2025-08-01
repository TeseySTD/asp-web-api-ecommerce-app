using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;

namespace Basket.Tests.Unit.Models.Cart.Entities;

public class ProductCartItemTest
{
    [Fact]
    public void WhenProductIsUpdated_ThenPropertiesAreChanged()
    {
        // Arrange
        var productCartItemToUpdate = ProductCartItem.Create(
            productId: ProductId.Create(Guid.NewGuid()).Value,
            title: ProductTitle.Create("Test").Value,
            stockQuantity: StockQuantity.Create(1).Value,
            price: ProductPrice.Create(1).Value,
            category: ProductCartItemCategory.Create(
                CategoryId.Create(Guid.NewGuid()).Value,
                CategoryName.Create("Test").Value
            )
        );

        var newTitle = ProductTitle.Create("Test2").Value;
        var newPrice = ProductPrice.Create(2).Value;
        var newCategory = ProductCartItemCategory.Create(
            CategoryId.Create(Guid.NewGuid()).Value,
            CategoryName.Create("Test2").Value
        );
        string[] imageUrls = ["img1", "img2"];
        
        // Act
        productCartItemToUpdate.Update(newTitle, newPrice, newCategory, imageUrls);
        
        // Assert
        Assert.Equal(newTitle, productCartItemToUpdate.Title);
        Assert.Equal(newPrice, productCartItemToUpdate.Price);
        Assert.Equal(newCategory, productCartItemToUpdate.Category);
        Assert.True(imageUrls.SequenceEqual(productCartItemToUpdate.ImageUrls));
    }
}