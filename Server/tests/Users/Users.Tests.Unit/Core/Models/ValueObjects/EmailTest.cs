using FluentAssertions;
using JetBrains.Annotations;
using Users.Core.Models.ValueObjects;

namespace Users.Tests.Unit.Core.Models.ValueObjects;

[TestSubject(typeof(Email))]
public class EmailTest
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@sub.domain.co")]
    [InlineData("first.last@domain.com")]
    [InlineData("email@domain.com")]
    [InlineData("email@sub.domain.com")]
    [InlineData("1234567890@domain.com")]
    [InlineData("_______@domain.com")]
    [InlineData("email@domain.name")]
    [InlineData("email@domain.co.jp")]
    public void Create_ValidEmail_ReturnsSuccess(string emailString)
    {
        // Act
        var result = Email.Create(emailString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(emailString);
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyOrWhitespaceEmail_ReturnsFailure_EmailIsRequired(string emailString)
    {
        // Act
        var result = Email.Create(emailString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == new Email.EmailRequiredError());
    }

    [Theory]
    [InlineData("invalid-email")] // No @
    [InlineData("user@.com")] // Dot before TLD
    [InlineData("user@com")] // Missing dot in domain
    [InlineData("user@domain..com")] // Double dot in domain
    [InlineData("user@domain")] // No TLD
    [InlineData(".username@domain.com")] // Dot at the beginning
    [InlineData("username.@domain.com")] // Dot at the end
    [InlineData("username@domain.c")] // TLD too short (assuming min 2 chars)
    [InlineData("user name@domain.com")] // Space in username
    [InlineData("user@domain com")] // Space in domain
    [InlineData("email@domain")] // No top-level domain
    [InlineData("email@.com")] // Domain starts with a dot
    [InlineData("email@-domain.com")] // Domain starts with a hyphen
    [InlineData("email@domain-.com")] // Domain ends with a hyphen
    public void Create_InvalidEmailFormat_ReturnsFailure_EmailIsNotValid(string emailString)
    {
        // Act
        var result = Email.Create(emailString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == new Email.EmailFormatError(emailString));
    }
}