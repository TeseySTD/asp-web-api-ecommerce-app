using Catalog.Core.Models.Categories.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Categories.ValueObjects;

public class CategoryIdTest
{
    [Fact]
    public void Create_EmptyId_ReturnsIdIsRequiredError()
    {
        // Arrange
        var id = Guid.Empty;
        
        // Act
        var result = CategoryId.Create(id);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CategoryId.IdIsRequiredError());
    }

    [Fact]
    public void Create_CorrectId_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var result = CategoryId.Create(id);
        
        // Assert 
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(id);
    }
}