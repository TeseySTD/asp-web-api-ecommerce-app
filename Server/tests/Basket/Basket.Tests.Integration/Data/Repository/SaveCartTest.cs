using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using FluentAssertions;

namespace Basket.Tests.Integration.Data.Repository;

public class SaveCartTest : IntegrationTest
{
    public SaveCartTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task WhenSaveCart_ThenSavesAndReturnsCart()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);

        // Act
        var result = await CartRepository.SaveCart(cart);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var loaded = await Session.LoadAsync<ProductCart>(userId, default);
        loaded.Should().BeEquivalentTo(cart);
    }
}