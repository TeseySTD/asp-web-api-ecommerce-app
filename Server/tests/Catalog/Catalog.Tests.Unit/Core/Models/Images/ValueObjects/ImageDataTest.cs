using Catalog.Core.Models.Images.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Images.ValueObjects;

public class ImageDataTest
{
    [Fact]
    public void Create_EmptyImageData_ReturnsImageDataRequiredError()
    {
        // Arrange
        byte[] data = [];
        
        // Act
        var result = ImageData.Create(data);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new ImageData.ImageDataRequiredError());
    }

    [Fact]
    public void Create_ImageDataIsTooBig_ReturnsOutOfLengthError()
    {
        // Arrange
        byte[] data = Enumerable.Repeat((byte)0xff, ImageData.MaxSize + 1).ToArray();
        
        // Act
        var result = ImageData.Create(data);
        
        //Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new ImageData.OutOfLengthError());
    }

    [Fact]
    public void Create_CorrectData_ReturnsSuccessResult()
    {
        // Arrange
        byte[] data = [1, 2, 3];
        
        // Act
        var result = ImageData.Create(data);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().BeEquivalentTo(data);
    }
}