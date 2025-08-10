using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Products;

public class ProductTest
{
    [Fact]
    public void Update_ValidData_ShouldUpdateProperties()
    {
        // Assert
        var productToUpdate = Product.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("Test Product").Value,
            ProductDescription.Create("Test Description").Value
        );
        var newTitle = ProductTitle.Create("New Test Title").Value;
        var newDescription = ProductDescription.Create("New Test Description").Value;
        
        // Act
        productToUpdate.Update(newTitle, newDescription);
        
        //Assert
        Assert.Equal(newTitle, productToUpdate.Title);
        Assert.Equal(newDescription, productToUpdate.Description);
    }
}