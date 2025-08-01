using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;

namespace Basket.Tests.Unit.Models.Cart.ValueObjects;
public class ProductTitleTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void WhenProductTitleIsEmpty_ThenReturnsFailureResult(string productTitle)
    {
        // Act
        var result = ProductTitle.Create(productTitle);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new ProductTitle.TitleRequiredError());
    }

    [Theory]
    [InlineData(ProductTitle.MaxTitleLength + 1)]
    [InlineData(ProductTitle.MinTitleLength - 1)]
    public void WhenProductTitleIsOutOfLength_ThenReturnsFailureResult(int productTitleLength)
    {
        // Arrange
        var title = string.Concat(Enumerable.Repeat("a", productTitleLength));
        
        // Act
        var result = ProductTitle.Create(title);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new ProductTitle.OutOfLengthError());
    }

    [Fact]
    public void WhenStringIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        var title = "test";
        
        // Act
        var result = ProductTitle.Create(title);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(title);
    }
}
