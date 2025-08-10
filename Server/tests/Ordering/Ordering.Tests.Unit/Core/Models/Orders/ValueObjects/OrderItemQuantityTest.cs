using FluentAssertions;
using Ordering.Core.Models.Orders.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders.ValueObjects;

public class OrderItemQuantityTest
{
    [Fact]
    public void Create_QuantityIsZero_ReturnsQuantityLessThenOneError()
    {
        // Act
        var result = OrderItemQuantity.Create(0);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new OrderItemQuantity.QuantityLessThenOneError());
    }

    [Fact]
    public void Create_QuantityIsCorrect_ReturnsSuccess()
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