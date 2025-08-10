using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;

namespace Basket.Tests.Unit.Models.Cart.ValueObjects;

public class StockQuantityTest
{
    [Fact]
    public void Create_QuantityIsLesserThanOne_ReturnsQuantityLesserThanOneError()
    {
        // Arrange
        uint quantity = 0;

        // Act
        var result = StockQuantity.Create(quantity);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new StockQuantity.QuantityLesserThanOneError());
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