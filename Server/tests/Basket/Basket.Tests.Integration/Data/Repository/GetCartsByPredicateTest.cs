using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using FluentAssertions;

namespace Basket.Tests.Integration.Data.Repository;

public class GetCartsByPredicateTest : IntegrationTest
{
    public GetCartsByPredicateTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task GetCartsByPredicate_NoCartsMatchPredicate_ReturnsEmptyList()
    {
        // Arrange
        var productId = ProductId.Create(Guid.NewGuid()).Value;

        // Act
        var result = await CartRepository
            .GetCartsByPredicate(c => c.Items.Any(i => i.Id.Value == productId.Value));


        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCartsByPredicate_CartsMatchPredicate_ReturnsCarts()
    {
        var userId1 = UserId.From(Guid.NewGuid());
        var userId2 = UserId.From(Guid.NewGuid());
        var productId = ProductId.Create(Guid.NewGuid()).Value;

        var cart1 = ProductCart.Create(userId1);
        cart1.AddItem(ProductCartItem.Create(
                productId,
                ProductTitle.Create("Test A").Value,
                StockQuantity.Create(2).Value,
                ProductPrice.Create(10m).Value,
                ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value,
                    CategoryName.Create("Test X").Value)
            )
        );
        var cart2 = ProductCart.Create(userId2);
        cart2.AddItem(ProductCartItem.Create(
                productId,
                ProductTitle.Create("Test B").Value,
                StockQuantity.Create(3).Value,
                ProductPrice.Create(15m).Value,
                ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value,
                    CategoryName.Create("Test Y").Value)
            )
        );

        Session.Store(cart1);
        Session.Store(cart2);
        await Session.SaveChangesAsync();

        // Act
        var result = await CartRepository
            .GetCartsByPredicate(c => c.Items.Any(i => i.Id.Value == productId.Value));
        
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        
    }
}