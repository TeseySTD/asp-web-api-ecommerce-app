using FluentAssertions;
using Users.Core.Models.ValueObjects;

namespace Users.Tests.Unit.Core.Models.ValueObjects;

public class EmailVerificationTokenIdTest
{
    
    [Fact]
    public void WhenIdIsEmpty_ThenReturnsFailureResult()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var result = EmailVerificationTokenId.Create(id);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is EmailVerificationTokenId.IdRequiredError);
    }

    [Fact]
    public void WhenIdIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = EmailVerificationTokenId.Create(id);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(id);
    }
}