using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;

namespace Basket.Tests.Unit.Models.Cart.ValueObjects;

public class CategoryNameTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void WhenNameIsEmptyOrWhiteSpace_ThenReturnsFailureResult(string categoryName)
    {
        // Act
        var result = CategoryName.Create(categoryName);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CategoryName.NameRequiredError());
    }

    [Fact]
    public void WhenNameIsOutOfLength_ThenReturnsFailureResult()
    {
        // Arrange
        var categoryName = new string('a', CategoryName.MaxNameLength + 1);
        
        // Act
        var result = CategoryName.Create(categoryName);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CategoryName.NameIsOutOfLengthError());
    }

    [Fact]
    public void WhenNameIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        var categoryName = "test";
        
        // Act
        var result = CategoryName.Create(categoryName);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(categoryName);
    }
}
