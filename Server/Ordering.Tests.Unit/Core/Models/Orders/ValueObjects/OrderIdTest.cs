using FluentAssertions;
using Ordering.Core.Models.Orders.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders.ValueObjects;

public class OrderIdTest
{
    [Fact]
    public void WhenIdIsEmpty_ThenReturnFailure()
    {
        // Act
        var result = OrderId.Create(Guid.Empty);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new OrderId.OrderIdRequiredError());
    }

    [Fact]
    public void WhenIdIsCorrect_ThenReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Acr
        var result = OrderId.Create(id);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(id);
    }
}