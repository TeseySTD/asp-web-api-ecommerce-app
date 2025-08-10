using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;

namespace Basket.Tests.Unit.Models.Cart.ValueObjects;
public class ProductIdTest
{
    [Fact]
    public void Create_EmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var id = Guid.Empty;
        
        // Act
        var result = ProductId.Create(id);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new ProductId.IdRequiredError());
    }

    [Fact]
    public void Create_CorrectId_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var result = ProductId.Create(id);
        
        //Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(id);
    }
}
