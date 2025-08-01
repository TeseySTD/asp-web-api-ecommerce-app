using Basket.API.Models.Cart.ValueObjects;

namespace Basket.Tests.Unit.Models.Cart.ValueObjects;

public class AddressTest
{
    [Theory]
    [InlineData("456 Oak Rd", "USA", "CA", "12345")]
    [InlineData("789 Elm Blvd", "USA", "NY", "12345-6789")]
    [InlineData("789 Elm Blvd", "USA", "NY", "")]
    public void WhenValidInput_ThenReturnsSuccess(string addressLine, string country, string state, string zip)
    {
        // Act
        var result = Address.Create(addressLine, country, state, zip);

        // Assert
        Assert.True(result.IsSuccess);
        var address = result.Value;
        Assert.Equal(addressLine, address.AddressLine);
        Assert.Equal(country, address.Country);
        Assert.Equal(state, address.State);
        Assert.Equal(zip, address.ZipCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void WhenEmptyAddressLine_ThenReturnsAddressLineRequiredError(string emptyLine)
    {
        // Act 
        var result = Address.Create(emptyLine, "USA", "CA", "12345");

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Errors);
        Assert.IsType<Address.AddressLineRequiredError>(error);
        Assert.Contains("Address line is required", error.Message);
    }

    [Theory]
    [InlineData("123 Main St", "USA", "CA", "1234")]
    [InlineData("123 Main St", "USA", "CA", "abcde")]
    [InlineData("123 Main St", "USA", "CA", "123456")]
    public void WhenWithInvalidZipCodeFormat_ThenReturnsZipCodeFormatError(string line, string country, string state,
        string zip)
    {
        // Act 
        var result = Address.Create(line, country, state, zip);

        // Assert
        Assert.True(result.IsFailure);
        var error = Assert.Single(result.Errors);
        Assert.IsType<Address.ZipCodeFormatError>(error);
        Assert.Contains($"Zip code '{zip}' is invalid", error.Description);
    }
}
