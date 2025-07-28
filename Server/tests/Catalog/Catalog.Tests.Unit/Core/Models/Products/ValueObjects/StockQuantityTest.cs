using Catalog.Core.Models.Products.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Products.ValueObjects;

public class StockQuantityTest
{
    [Fact]
    public void WhenQuantityIsLesserThanZero_ThenReturnsFailureResult()
    {
        // Arrange
        var quantity = -1;
        
        // Act
        var result = StockQuantity.Create(quantity);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new StockQuantity.QuantityLesserThanZeroError());
    }

    [Fact]
    public void WhenQuantityIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        uint quantity = 1;
        
        // Act
        var result = StockQuantity.Create(quantity);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(quantity);
    }
}