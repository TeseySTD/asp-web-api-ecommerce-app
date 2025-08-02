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

    [Fact]
    public async Task WhenNewCart_ThenAddsItemAndReturnsSuccessfullResult()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        var item = ProductCartItem.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("T").Value,
            StockQuantity.Create(1).Value,
            ProductPrice.Create(5m).Value,
            ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value, CategoryName.Create("C").Value)
        );

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
    public async Task WhenProductIsDuplicate_ThenReturnsFailureResult()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        var item = ProductCartItem.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("T").Value,
            StockQuantity.Create(1).Value,
            ProductPrice.Create(5m).Value,
            ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value, CategoryName.Create("C").Value)
        );

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
    public async Task WhenCartDoesNotExist_ThenCreatesNewAndReturnsSuccess()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var item = ProductCartItem.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("T").Value,
            StockQuantity.Create(1).Value,
            ProductPrice.Create(5m).Value,
            ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value, CategoryName.Create("C").Value)
        );

        // Act
        var result = await CartRepository.StoreProductInCart(userId, item);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var loaded = await Session.LoadAsync<ProductCart>(userId);
        loaded.Should().NotBeNull();
        loaded.Items.Should().ContainSingle(i => i.Id == item.Id);
    }
}