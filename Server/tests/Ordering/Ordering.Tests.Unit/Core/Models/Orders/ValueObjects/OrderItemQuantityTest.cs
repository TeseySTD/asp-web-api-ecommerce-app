using FluentAssertions;
using Ordering.Core.Models.Orders.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders.ValueObjects;

public class OrderItemQuantityTest
{
    [Fact]
    public void WhenQuantityIsZero_ThenShouldReturnFailure()
    {
        // Act
        var result = OrderItemQuantity.Create(0);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new OrderItemQuantity.QuantityLessThenOneError());
    }

    [Fact]
    public void WhenQuantityCorrect_ThenShouldReturnSuccess()
    {
        // Arrange
        uint q = 10;
        
        // Act
        var result = OrderItemQuantity.Create(q);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(q);
    }
}