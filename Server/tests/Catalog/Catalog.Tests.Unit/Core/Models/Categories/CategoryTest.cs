using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.Events;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Categories;

public class CategoryTest
{
    [Fact]
    public void WhenCreateIsCalledWithValidData_ThenCategoryCreatedAndEventIsDispatched()
    {
        // Act
        var category = Category.Create(
            CategoryName.Create("Name").Value,
            CategoryDescription.Create("Description").Value
        );
        
        // Assert
        category.DomainEvents.Should().ContainSingle(e => e is CategoryCreatedDomainEvent)
            .Which.As<CategoryCreatedDomainEvent>().CategoryId.Should().Be(category.Id);
    }

    [Fact]
    public void WhenUpdateIsCalledWithValidData_ThenPropertiesAreUpdatedAndCategoryUpdatedEventIsDispatched()
    {
        // Arrange
        var original = Category.Create(
            CategoryName.Create("OldName").Value,
            CategoryDescription.Create("OldDescription").Value
        );
        var newName = CategoryName.Create("NewName").Value;
        var newDescription = CategoryDescription.Create("NewDesc").Value;

        // Act
        original.Update(newName, newDescription);

        // Assert
        original.Name.Should().Be(newName);
        original.Description.Should().Be(newDescription);

        original.DomainEvents.Should().ContainSingle(e => e is CategoryUpdatedDomainEvent)
            .Which.As<CategoryUpdatedDomainEvent>().Category.Should().BeSameAs(original);
    }

    [Fact]
    public void WhenAddImageAndImagesUnderLimit_ThenImageIsAddedToList()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Name").Value,
            CategoryDescription.Create("Desc").Value
        );
        var img = Image.Create(
            FileName.Create("img.jpg").Value,
            ImageData.Create(new byte[] { 1, 2, 3 }).Value,
            ImageContentType.PNG
        );

        // Act
        category.AddImage(img);

        // Assert
        category.Images.Should().HaveCount(1);
        category.Images[0].CategoryId.Should().Be(category.Id);
        category.Images[0].Id.Should().Be(img.Id);
    }

    [Fact]
    public void WhenRemoveImageAndImageExists_Then_mageIsRemoved_And_CategoryUpdatedEventDispatched()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Name").Value,
            CategoryDescription.Create("Desc").Value
        );
        var imgId = ImageId.Create(Guid.NewGuid()).Value;
        var img = Image.Create(
            FileName.Create("img.jpg").Value,
            ImageData.Create(new byte[] { 1, 2, 3 }).Value,
            ImageContentType.PNG
        );
        category.AddImage(img);

        // Act
        category.RemoveImage(imgId);

        // Assert
        category.Images.Should().NotContain(i => i.Id == imgId);

        category.DomainEvents.Should().ContainSingle(e => e is CategoryUpdatedDomainEvent)
            .Which.As<CategoryUpdatedDomainEvent>().Category.Should().BeSameAs(category);
    }
}