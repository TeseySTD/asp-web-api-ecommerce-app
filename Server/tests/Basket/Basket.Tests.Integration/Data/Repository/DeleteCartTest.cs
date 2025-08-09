using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using FluentAssertions;

namespace Basket.Tests.Integration.Data.Repository;

public class DeleteCartTest : IntegrationTest
{
    public DeleteCartTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task DeleteCart_CartNotExist_ReturnsCartWithUserIdNotFoundError()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());

        // Act
        var result = await CartRepository.DeleteCart(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.CartWithUserIdNotFoundError);
    }

    [Fact]
    public async Task DeleteCart_CartExists_ThenRemovesCart()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        
        Session.Store(cart);
        await Session.SaveChangesAsync();

        // Act
        var result = await CartRepository.DeleteCart(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var loaded = await Session.LoadAsync<ProductCart>(userId);
        loaded.Should().BeNull();
    }
}