using FluentAssertions;
using JetBrains.Annotations;
using Users.Core.Models.ValueObjects;

namespace Users.Tests.Unit.Application.Models.ValueObjects;

[TestSubject(typeof(HashedPassword))]
public class HashedPasswordTest
{

    [Theory]
    [InlineData("awdawdawdw")]
    [InlineData("hashashash")]
    public void Create_ValidHash_ReturnsSuccess(string password)
    {
        // Act
        var createHashedPasswordResult = HashedPassword.Create(password);
        
        // Assert
        createHashedPasswordResult.IsSuccess.Should().BeTrue();
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespace_ReturnsFailure_HashPasswordCannnotBeEmpty(string password)
    {
        // Act
        var createHashedPasswordResult = HashedPassword.Create(password);
        
        // Assert
        createHashedPasswordResult.IsFailure.Should().BeTrue();
        createHashedPasswordResult.Errors.Should().ContainSingle(e => e.Description == "Hashed password cannot be empty");
    }
}