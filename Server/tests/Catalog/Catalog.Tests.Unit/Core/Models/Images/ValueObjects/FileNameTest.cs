using Catalog.Core.Models.Images.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Images.ValueObjects;

public class FileNameTest
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_FileNameIsWhitespaceOrEmpty_ReturnsFileNameRequiredError(string filename)
    {
        // Act
        var result = FileName.Create(filename);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new FileName.FileNameRequiredError());
    }

    [Fact]
    public void Create_FileNameIsOutOfLength_ReturnsOutOfLengthError()
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
    public void Create_CorrectFileName_ReturnsSuccessResult()
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