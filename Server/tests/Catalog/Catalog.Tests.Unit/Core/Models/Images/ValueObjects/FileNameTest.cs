using Catalog.Core.Models.Images.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Images.ValueObjects;

public class FileNameTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void WhenFileNameIsWhitespaceOrEmpty_ThenReturnsFailureResult(string filename)
    {
        // Act
        var result = FileName.Create(filename);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new FileName.FileNameRequiredError());
    }

    [Fact]
    public void WhenFileNameIsOutOfLength_ThenReturnsFailureResult()
    {
        // Arrange
        var filename = new string('a', FileName.MaxLength + 1);
        
        // Act
        var result = FileName.Create(filename);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new FileName.OutOfLengthError());
    }

    [Fact]
    public void WhenFileNameIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        var filename = "test-file-name";
        
        // Act
        var result = FileName.Create(filename);
        
        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(filename);
    }
}