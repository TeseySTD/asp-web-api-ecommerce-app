using Catalog.Core.Models.Images.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Images.ValueObjects;

public class ImageIdTest
{
    [Fact]
    public void Create_EmptyId_ReturnsImageIdRequiredError()
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
    public void Create_CorrectId_ReturnsSuccessResult()
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