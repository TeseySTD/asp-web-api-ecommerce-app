using FluentAssertions;
using Users.Core.Models.ValueObjects;

namespace Users.Tests.Unit.Core.Models.ValueObjects;

public class UserIdTest
{
    [Fact]
    public void Create_IdIsEmpty_ReturnsIdRequiredError()
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
    public void Create_IdIsCorrect_ReturnsSuccessResult()
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