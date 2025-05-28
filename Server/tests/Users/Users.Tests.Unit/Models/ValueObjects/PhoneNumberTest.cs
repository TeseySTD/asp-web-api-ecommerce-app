using FluentAssertions;
using JetBrains.Annotations;
using Users.Core.Models.ValueObjects;

namespace Users.Tests.Unit.Models.ValueObjects;

[TestSubject(typeof(PhoneNumber))]
public class PhoneNumberTest
{
    [Theory]
    [InlineData("+380991234567")] // Standard international format
    [InlineData("0991234567")] // Standard national format
    [InlineData("099-123-45-67")] // With hyphens
    [InlineData("(099)123-45-67")] // With parentheses
    [InlineData("+38(099)1234567")] // Mixed format
    [InlineData("380991234567")] // Without '+'
    [InlineData("80991234567")] // Old format (if supported by regex)
    public void Create_ValidPhoneNumber_ReturnsSuccess(string phoneNumberString)
    {
        // Act
        var result = PhoneNumber.Create(phoneNumberString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Value.Should().Be(phoneNumberString);
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")] // Whitespace only
    public void Create_EmptyOrWhitespacePhoneNumber_ReturnsFailure_PhoneNumberIsRequired(string phoneNumberString)
    {
        // Act
        var result = PhoneNumber.Create(phoneNumberString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.Message == "Phone number is required" && e.Description == "Phone number must be not null or empty.");
    }

    // Test for failure with various invalid phone number formats
    [Theory]
    [InlineData("12345")] // Too short
    [InlineData("099123456")] // Too short (missing a digit)
    [InlineData("+3809912345678")] // Too long (extra digit)
    [InlineData("abcde")] // Non-numeric characters
    [InlineData("099123456x")] // Contains invalid characters
    [InlineData("+1234567890")] // Incorrect country code (not Ukrainian)
    [InlineData("099 123 456")] // Missing last two digits
    [InlineData("09912345678")] // One digit too many
    public void Create_InvalidPhoneNumberFormat_ReturnsFailure_PhoneNumberIsIncorrect(string phoneNumberString)
    {
        // Act
        var result = PhoneNumber.Create(phoneNumberString);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e =>
            e.Message == "Phone number is incorrect." && e.Description == "Phone number is not a valid phone number.");
    }
}