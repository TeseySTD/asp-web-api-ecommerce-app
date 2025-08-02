using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using FluentAssertions;

namespace Basket.Tests.Integration.Data.Repository;

public class RemoveProductFromCartTest : IntegrationTest
{
    public RemoveProductFromCartTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task WhenCartNotExist_ThenReturnsFailureResult()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var productId = ProductId.From(Guid.NewGuid());

        // Act
        var result = await CartRepository.RemoveProductFromCart(userId, productId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.CartWithUserIdNotFoundError);
    }

    [Fact]
    public async Task WhenItemNotExist_ThenReturnsFailureResult()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        
        Session.Store(cart);
        await Session.SaveChangesAsync();
        
        var productId = ProductId.Create(Guid.NewGuid()).Value;

        // Act
        var result = await CartRepository.RemoveProductFromCart(userId, productId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.ProductInCartNotFound);
    }

    [Fact]
    public async Task WhenItemExists_ThenRemovesItemAndReturnsSuccessResult()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        var productId = ProductId.Create(Guid.NewGuid()).Value;
        var item = ProductCartItem.Create(
            productId,
            ProductTitle.Create("T").Value,
            StockQuantity.Create(2).Value,
            ProductPrice.Create(10m).Value,
            ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value, CategoryName.Create("C").Value)
        );

        cart.AddItem(item);
        Session.Store(cart);
        await Session.SaveChangesAsync();

        // Act
        var result = await CartRepository.RemoveProductFromCart(userId, productId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var loaded = await Session.LoadAsync<ProductCart>(userId);
        loaded.Should().NotBeNull();
        loaded.Items.Should().BeEmpty();
    }
}