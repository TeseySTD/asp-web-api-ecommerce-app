using FluentAssertions;
using Ordering.Core.Models.Products.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Products.ValueObjects;

public class ProductDescriptionTest
{
    [Fact]
    public void Create_DescriptionIsEmpty_ReturnsProductDescriptionRequiredError()
    {
        // Act 
        var result = ProductDescription.Create("");
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == new ProductDescription.ProductDescriptionRequiredError());
    }

    [Fact]
    public void Create_DescriptionIsOutOfLength_ReturnsProductDescriptionOutOfLengthError()
    {
        // Arrange
        var str = new string('a', ProductDescription.MaxDescriptionLength + 1);
        
        // Act
        var result = ProductDescription.Create(str);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == new ProductDescription.ProductDescriptionOutOfLengthError());

    }

    [Fact]
    public void Create_DataIsCorrect_ReturnSuccess()
    {
        // Arrange
        var data = "awdawdawdawdawd";
        
        // Act
        var result = ProductDescription.Create(data);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(data);
    }
}