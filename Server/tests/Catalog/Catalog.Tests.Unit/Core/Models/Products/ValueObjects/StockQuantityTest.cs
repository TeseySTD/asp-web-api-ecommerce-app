using Catalog.Core.Models.Products.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Products.ValueObjects;

public class StockQuantityTest
{
    [Fact]
    public void Create_QuantityIsLessThanZero_ReturnsQuantityLesserThanZeroError()
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
    public void Create_CorrectQuantity_ReturnsSuccessResult()
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