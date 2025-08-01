using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;

namespace Basket.Tests.Unit.Models.Cart.ValueObjects;
public class ProductPriceTest
{
    [Fact] 
    public void WhenPriceIsOutOfRange_ThenReturnsFailureResult()
    {
        // Arrange
        var price = ProductPrice.MinPrice - 0.01m;
        
        // Act
        var result = ProductPrice.Create(price);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new ProductPrice.OutOfRangeError());
    }

    [Fact]
    public void WhenPriceIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        var price = 10m;
        
        // Act
        var result = ProductPrice.Create(price);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(price);
    }
}
