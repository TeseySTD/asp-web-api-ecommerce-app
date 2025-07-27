using FluentAssertions;
using Ordering.Core.Models.Orders.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders.ValueObjects;

public class OrderItemIdTest
{
    [Fact]
    public void WhenIdIsEmpty_ThenReturnsFailure()
    {
        // Act
        var result = OrderItemId.Create(Guid.Empty);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new OrderItemId.OrderItemIdRequiredError());
    }

    [Fact]
    public void WhenIdIsCorrect_ThenReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var result = OrderItemId.Create(id);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(id);
    }
}