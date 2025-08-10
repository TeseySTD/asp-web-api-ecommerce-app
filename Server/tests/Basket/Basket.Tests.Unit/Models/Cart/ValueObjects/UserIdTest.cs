using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;

namespace Basket.Tests.Unit.Models.Cart.ValueObjects;

public class UserIdTest
{
    [Fact]
    public void Create_EmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var userId = Guid.Empty;
        
        // Act
        var result = UserId.Create(userId);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is UserId.IdRequiredError);
    }

    [Fact]
    public void Create_CorrectId_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act
        var result = UserId.Create(userId);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(userId);
    }
}