using FluentAssertions;
using JetBrains.Annotations;
using Users.Core.Models.ValueObjects;

namespace Users.Tests.Unit.Core.Models.ValueObjects;

[TestSubject(typeof(UserName))]
public class UserNameTest
{
    [Theory]
    [InlineData("John Doe")]
    [InlineData("Alice")]
    [InlineData("B")] 
    public void Create_ValidString_ReturnsSuccess(string validName)
    {
        // Act
        var userNameCreateResult = UserName.Create(validName);
        
        // Assert
        Assert.True(userNameCreateResult.IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespaceString_ReturnsUserNameRequiredError(string userNameString)
    {
        // Act
        var userNameCreateResult = UserName.Create(userNameString);
        
        // Assert
        userNameCreateResult.IsFailure.Should().BeTrue();
        userNameCreateResult.Errors.Should().Contain(new UserName.UserNameRequiredError());
    }

    [Fact]
    public void Create_MaxNameLength_ReturnsUserNameOutOfRangeError()
    {
        // Arrange
        var userNameString = new String('a', UserName.MaxNameLength + 1);
        
        // Act
        var userNameCreateResult = UserName.Create(userNameString);
        
        // Assert
        userNameCreateResult.IsFailure.Should().BeTrue();
        userNameCreateResult.Errors.Should().Contain(new UserName.UserNameOutOfRangeError());
    }
    
    [Fact]
    public void Create_MinNameLength_ReturnsFailureAndAppropriateError()
    {
        // Arrange
        var userNameString = new String('a', UserName.MinNameLength - 1);
        
        // Act
        var userNameCreateResult = UserName.Create(userNameString);
        
        // Assert
        userNameCreateResult.IsFailure.Should().BeTrue();
        userNameCreateResult.Errors.Should().Contain(e => e == new UserName.UserNameOutOfRangeError() || e == new UserName.UserNameRequiredError());
    }
}