using FluentAssertions;
using Ordering.Core.Models.Products.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Products.ValueObjects;

public class ProductTitleTest
{
    [Fact]
    public void Create_ProductTitleIsEmpty_ReturnsProductTitleRequiredError()
    {
        // Act
        var result = ProductTitle.Create(string.Empty);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == new ProductTitle.ProductTitleRequiredError());
    }

    [Fact]
    public void Create_ProductTitleIsOutOfRange_ReturnsProductTitleOutOfRangeError()
    {
        // Act
        var result = ProductTitle.Create("a");
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == new ProductTitle.ProductTitleOutOfRangeError());
    }

    [Fact]
    public void Create_DataIsValid_ReturnsSuccess()
    {
        // Arrange
        var title = "title";
        
        // Act
        var result = ProductTitle.Create(title);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title);
    }
}