using Catalog.Core.Models.Products.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Products.ValueObjects;

public class SellerIdTest
{
    [Fact]
    public void Create_EmptyId_ReturnsIdRequiredError()
    {
        // Arrange
        var id = Guid.Empty;

        // Act
        var result = SellerId.Create(id);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new SellerId.IdRequiredError());
    }

    [Fact]
    public void Create_CorrectId_ReturnsSuccessResult()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = ProductId.Create(id);

        //Assert
        Assert.True(result.IsSuccess);
        result.Value.Value.Should().Be(id);
    }
}