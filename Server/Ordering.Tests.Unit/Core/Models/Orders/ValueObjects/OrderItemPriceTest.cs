using FluentAssertions;
using Ordering.Core.Models.Orders.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders.ValueObjects;

public class OrderItemPriceTest
{
    [Theory]
    [InlineData(0.05)]
    [InlineData(1000000000)]
    public void WhenOrderItemPriceIsOutOfRange_ThenReturnFailure(decimal price)
    {
        // Act
        var result = OrderItemPrice.Create(price);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new OrderItemPrice.OrderItemPriceOutOfRangeError());
    }

    [Fact]
    public void WhenDataIsCorrect_ThenReturnSuccess()
    {
        // Arrange
        var price = 1m;
        
        // Act
        var result = OrderItemPrice.Create(price);
        
        //Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(price, result.Value.Value);
    }
}