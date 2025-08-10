using FluentAssertions;
using Ordering.Core.Models.Orders.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders.ValueObjects;
public class PaymentTest
{
    [Theory]
    [InlineData("John Doe", "4111111111111111", "12/25", "123", "Visa")]
    [InlineData("Jane Smith", "5555555555554444", "01/26", "456", "MasterCard")]
    [InlineData("Bob Johnson", "4000000000000002", null, "789", null)]
    public void Create_ValidInput_ReturnsSuccess(string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod)
    {
        // Act
        var result = Payment.Create(cardName, cardNumber, expiration, cvv, paymentMethod);
        
        // Assert
        Assert.True(result.IsSuccess);
        var payment = result.Value;
        Assert.Equal(cardName, payment.CardName);
        Assert.Equal(cardNumber, payment.CardNumber);
        Assert.Equal(expiration ?? "Unknown", payment.Expiration);
        Assert.Equal(cvv, payment.CVV);
        Assert.Equal(paymentMethod ?? "Unknown", payment.PaymentMethod);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_CardNameIsEmptyOrWhitespace_ReturnsCardNameRequiredError(string emptyCardName)
    {
        // Act
        var result = Payment.Create(emptyCardName, "4111111111111111", "12/25", "123", "Visa");
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().Contain(new Payment.CardNameRequiredError()); 
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_CardNumberIsEmptyOrWhitespace_ReturnsCardNumberRequiredError(string emptyCardNumber)
    {
        // Act
        var result = Payment.Create("John Doe", emptyCardNumber, "12/25", "123", "Visa");
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().Contain(new Payment.CardNumberRequiredError());
    }

    [Theory]
    [InlineData("1234567890123456")] // Invalid format
    [InlineData("378282246310005")] // Amex (not supported)
    [InlineData("411111111111111")] // Too short Visa
    [InlineData("41111111111111111")] // Too long Visa
    public void Create_InvalidCardNumber_ReturnsInvalidCardNumberError(string invalidCardNumber)
    {
        // Act
        var result = Payment.Create("John Doe", invalidCardNumber, "12/25", "123", "Visa");
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().Contain(new Payment.InvalidCardNumberError(invalidCardNumber));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_CVVIsEmptyOrWhitespace_ReturnsCVVRequiredError(string emptyCVV)
    {
        // Act
        var result = Payment.Create("John Doe", "4111111111111111", "12/25", emptyCVV, "Visa");
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().Contain(new Payment.CVVRequiredError());
    }

    [Theory]
    [InlineData("12")] // Too short
    [InlineData("1234")] // Too long
    [InlineData("12345")] // Way too long
    public void Create_InvalidCVVLength_ReturnsCVVLengthError(string invalidCVV)
    {
        // Act
        var result = Payment.Create("John Doe", "4111111111111111", "12/25", invalidCVV, "Visa");
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().Contain(new Payment.CVVLengthError(invalidCVV.Length));
    }

    [Theory]
    [InlineData("4111111111111111", "12/25", "123")] // Valid Visa
    [InlineData("5555555555554444", "01/26", "456")] // Valid MasterCard
    [InlineData("4000000000000002", "03/27", "789")] // Another valid Visa
    public void Create_ValidVisaOrMasterCard_ReturnsSuccess(string cardNumber, string expiration, string cvv)
    {
        // Act
        var result = Payment.Create("Test User", cardNumber, expiration, cvv, "Test");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(cardNumber, result.Value.CardNumber);
    }

    [Fact]
    public void Create_NullExpirationAndPaymentMethod_ShouldSetDefaultValuesAndReturnsSuccess()
    {
        // Act
        var result = Payment.Create("John Doe", "4111111111111111", null, "123", null);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Unknown", result.Value.Expiration);
        Assert.Equal("Unknown", result.Value.PaymentMethod);
    }
}
