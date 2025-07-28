using Catalog.Core.Models.Images.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Images.ValueObjects;

public class ImageIdTest
{
    [Fact]
    public void WhenIdIsEmpty_ThenReturnsFailureResult()
    {
        // Arrange
        var id = Guid.Empty;
        
        // Act
        var result = ImageId.Create(id);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new ImageId.ImageIdRequiredError());
    }

    [Fact]
    public void WhenIdIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var result = ImageId.Create(id);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(id);
    }
}