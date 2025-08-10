using Catalog.Core.Models.Products.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Products.ValueObjects;

public class ProductPriceTest
{
    [Fact] 
    public void Create_PriceIsOutOfRange_ReturnsOutOfRangeError()
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
    public void Create_CorrectPrice_ReturnsSuccessResult()
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