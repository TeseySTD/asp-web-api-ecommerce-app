using FluentAssertions;
using Ordering.Core.Models.Orders.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders.ValueObjects;

public class CustomerIdTest
{
    [Fact]
    public void WhenCustomerIdIsEmpty_ThenReturnsFailure()
    {
        // Act 
        var result = CustomerId.Create(Guid.Empty);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CustomerId.CustomerIdRequiredError());
    }

    [Fact]
    public void WhenIdIsCorrect_ThenReturnsSuccess()
    {
        // Arrange
        var id = Guid.NewGuid();
        
        // Act
        var result = CustomerId.Create(id);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(result.Value.Value, id);
    }
}