using FluentAssertions;
using Ordering.Core.Models.Products.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Products.ValueObjects;

public class ProductIdTest
{
    [Fact]
    public void Create_ProductIdIsEmpty_ReturnProductIdRequiredError()
    {
        // Act
        var result = ProductId.Create(Guid.Empty);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == new ProductId.ProductIdRequiredError());
    }

    [Fact]
    public void Create_IdIsCorrect_ReturnSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act 
        var result = ProductId.Create(id);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(id);
    }
}