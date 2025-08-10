using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using FluentAssertions;

namespace Basket.Tests.Integration.Data.Repository;

public class StoreProductInCartTest : IntegrationTest
{
    public StoreProductInCartTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    private ProductCartItem CreateTestProductCartItem() =>
        ProductCartItem.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("Test").Value,
            StockQuantity.Create(1).Value,
            ProductPrice.Create(5m).Value,
            ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value, CategoryName.Create("Test").Value)
        );

    [Fact]
    public async Task StoreProductInCart_NewCart_ShouldAddItemAndReturnsSuccessfullResult()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        var item = CreateTestProductCartItem();

        Session.Store(cart);
        await Session.SaveChangesAsync();

        // Act
        var result = await CartRepository.StoreProductInCart(userId, item);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var loaded = await Session.LoadAsync<ProductCart>(userId);
        loaded.Should().NotBeNull();
        loaded.Items.Should().ContainSingle(i => i.Id == item.Id);
    }

    [Fact]
    public async Task StoreProductInCart_ProductIsDuplicate_ReturnsProductAlreadyInCartError()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        var item = CreateTestProductCartItem();
        cart.AddItem(item);
        Session.Store(cart);
        await Session.SaveChangesAsync();

        // Act
        var result = await CartRepository.StoreProductInCart(userId, item);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.ProductAlreadyInCartError);
    }

    [Fact]
    public async Task StoreProductInCart_CartDoesNotExist_ShouldCreateNewAndReturnsSuccess()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var item = CreateTestProductCartItem();

        // Act
        var result = await CartRepository.StoreProductInCart(userId, item);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var loaded = await Session.LoadAsync<ProductCart>(userId);
        loaded.Should().NotBeNull();
        loaded.Items.Should().ContainSingle(i => i.Id == item.Id);
    }
}