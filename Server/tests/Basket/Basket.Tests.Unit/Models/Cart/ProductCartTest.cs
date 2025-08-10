using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;

namespace Basket.Tests.Unit.Models.Cart;

public class ProductCartTest
{
    private readonly UserId _userId = UserId.Create(Guid.NewGuid()).Value;

    private ProductCartItem CreateTestProductCartItem(ProductId id) => ProductCartItem.Create(
        id,
        ProductTitle.Create("test").Value,
        StockQuantity.Create(2).Value,
        ProductPrice.Create(1).Value,
        ProductCartItemCategory.Create(
            CategoryId.Create(Guid.NewGuid()).Value,
            CategoryName.Create("Test").Value
        )
    );

    [Fact]
    public void Create_NoItems_ShouldInitializeEmptyCart()
    {
        // Act
        var cart = ProductCart.Create(_userId);

        // Assert
        cart.Items.Should().BeEmpty();
        cart.TotalPrice.Should().Be(0m);
        cart.HasItem(ProductId.Create(Guid.NewGuid()).Value).Should().BeFalse();
    }

    [Fact]
    public void Create_WithItems_ShouldInitializeCartWithItems()
    {
        // Arrange
        var id1 = ProductId.Create(Guid.NewGuid()).Value;
        var item1 = CreateTestProductCartItem(id1);

        var id2 = ProductId.Create(Guid.NewGuid()).Value;
        var item2 = CreateTestProductCartItem(id2);

        var items = new List<ProductCartItem> { item1, item2 };

        // Act
        var cart = ProductCart.Create(_userId, items);

        // Assert
        cart.Items.Should().HaveCount(2);
        cart.HasItem(id1).Should().BeTrue();
        cart.HasItem(id2).Should().BeTrue();
        cart.TotalPrice.Should().Be(item1.Price.Value * item1.StockQuantity.Value + item2.Price.Value * item2.StockQuantity.Value);
    }

    [Fact]
    public void AddItem_WithItem_ShouldAddSingleItemToCart()
    {
        // Arrange
        var cart = ProductCart.Create(_userId);
        var id = ProductId.Create(Guid.NewGuid()).Value;
        var item = CreateTestProductCartItem(id);

        // Act
        cart.AddItem(item);

        // Assert
        cart.Items.Should().ContainSingle(i => i == item);
        cart.TotalPrice.Should().Be(item.StockQuantity.Value * item.Price.Value);
    }

    [Fact]
    public void AddItems_WithItems_ShouldAddMultipleItemsToCart()
    {
        // Arrange
        var cart = ProductCart.Create(_userId);
        var itemList = new List<ProductCartItem>
        {
            CreateTestProductCartItem(ProductId.Create(Guid.NewGuid()).Value),
            CreateTestProductCartItem(ProductId.Create(Guid.NewGuid()).Value)
        };

        // Act
        cart.AddItems(itemList);

        // Assert
        cart.Items.Should().HaveCount(2);
        cart.TotalPrice.Should().Be(itemList.Aggregate(0, (n, i) => n + (int)(i.StockQuantity.Value * i.Price.Value)));
    }

    [Fact]
    public void RemoveItem_WithItemId_ShouldRemoveItemById()
    {
        // Arrange
        var idToRemove = ProductId.Create(Guid.NewGuid()).Value;
        var item1 = CreateTestProductCartItem(idToRemove);
        var item2 = CreateTestProductCartItem(ProductId.Create(Guid.NewGuid()).Value);
        var cart = ProductCart.Create(_userId, [item1, item2]);

        // Act
        cart.RemoveItem(idToRemove);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.HasItem(idToRemove).Should().BeFalse();
    }

    [Fact]
    public void HasItem_WithItemId_ShouldReturnCorrectBoolean()
    {
        // Arrange
        var presentId = ProductId.Create(Guid.NewGuid()).Value;
        var missingId = ProductId.Create(Guid.NewGuid()).Value;
        var cart = ProductCart.Create(_userId, [CreateTestProductCartItem(presentId)]);

        // Act
        var presentResult = cart.HasItem(presentId);
        var missingResult = cart.HasItem(missingId);

        // Assert
        presentResult.Should().BeTrue();
        missingResult.Should().BeFalse();
    }
}