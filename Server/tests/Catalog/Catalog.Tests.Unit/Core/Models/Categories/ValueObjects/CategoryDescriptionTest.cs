using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Categories.ValueObjects;

public class CategoryDescriptionTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_DescriptionIsEmptyOrWhitespace_ReturnsDescriptionRequiredError(string description)
    {
        // Act
        var result = CategoryDescription.Create(description);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CategoryDescription.DescriptionRequiredError());
    }

    [Theory]
    [InlineData(CategoryDescription.MinDescriptionLength - 1)]
    [InlineData(CategoryDescription.MaxDescriptionLength + 1)]
    public void Create_DescriptionIsOutOfLenght_ReturnsOutOfLengthError(int length)
    {
        // Arrange
        var description = new string('a', length);
        
        // Act
        var result = CategoryDescription.Create(description);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CategoryDescription.OutOfLengthError());
    }

    [Fact]
    public void Create_CorrectDescriptiont_ReturnsSuccessResult()
    {
        // Arrange
        var description = "test description";
        
        // Act
        var result = CategoryDescription.Create(description);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(description);
    }
    
}