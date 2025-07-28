using Catalog.Core.Models.Products.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Products.ValueObjects;

public class ProductDescriptionTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    public void WhenDescriptionIsNullEmptyOrWhiteSpace_ThenReturnsFailure(string description)
    {
        // Act
        var productDescription = ProductDescription.Create(description);
        
        // Assert
        Assert.True(productDescription.IsFailure);
        productDescription.Errors.Should().ContainSingle(e => e == new ProductDescription.DescriptionRequiredError());
    }

    [Theory]
    [InlineData(ProductDescription.MaxDescriptionLength + 1)]
    [InlineData(ProductDescription.MinDescriptionLength - 1)]
    public void WhenDescriptionIsOutOfLength_ThenReturnsFailure(int descriptionLength)
    {
        // Arrange
        var stringDescription = string.Concat(Enumerable.Repeat('a', descriptionLength));
        
        // Act
        var productDescription = ProductDescription.Create(stringDescription);
        
        // Assert
        Assert.True(productDescription.IsFailure);
        productDescription.Errors.Should().ContainSingle(e => e == new ProductDescription.OutOfLengthError());
    }

    [Fact]
    public void WhenStringIsCorrect_ThenReturnsSuccess()
    {
        // Arrange
        var stringDescription = "This is a test";
        
        // Act
        var productDescription = ProductDescription.Create(stringDescription);
        
        // Assert
        Assert.True(productDescription.IsSuccess);
        productDescription.Value.Value.Should().Be(stringDescription);
    }
}