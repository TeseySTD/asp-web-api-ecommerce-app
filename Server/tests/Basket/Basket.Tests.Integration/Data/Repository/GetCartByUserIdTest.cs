using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using FluentAssertions;

namespace Basket.Tests.Integration.Data.Repository;

public class GetCartByUserIdTest : IntegrationTest
{
    public GetCartByUserIdTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task GetCartByUserId_CartDoesNotExist_ReturnsCartWithUserIdNotFoundError()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());

        // Act
        var result = await CartRepository.GetCartByUserId(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.CartWithUserIdNotFoundError);
    }

    [Fact]
    public async Task  GetCartByUserId_CartExists_ReturnsCart()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var cart = ProductCart.Create(userId);
        cart.AddItem(
            ProductCartItem.Create(
                ProductId.Create(Guid.NewGuid()).Value,
                ProductTitle.Create("T").Value,
                StockQuantity.Create(1).Value,
                ProductPrice.Create(5m).Value,
                ProductCartItemCategory.Create(
                    CategoryId.Create(Guid.NewGuid()).Value,
                    CategoryName.Create("Cat").Value)
            )
        );

        Session.Store(cart);
        await Session.SaveChangesAsync();

        // Act
        var result = await CartRepository.GetCartByUserId(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(userId);
        result.Value.Items.Should().HaveCount(1);
    }
}