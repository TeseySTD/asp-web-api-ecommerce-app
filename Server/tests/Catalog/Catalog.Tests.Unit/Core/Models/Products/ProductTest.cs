using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.Events;
using Catalog.Core.Models.Products.ValueObjects;
using FluentAssertions;

namespace Catalog.Tests.Unit.Core.Models.Products;

public class ProductTest
{
    [Fact]
    public void Create_ValidData_ShouldCreateProductAndDispatchProductCreatedDomainEvent()
    {
        // Act
        var product = Product.Create(
            ProductTitle.Create("Test Title").Value,
            ProductDescription.Create("Test Description").Value,
            ProductPrice.Create(100).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId: CategoryId.Create(Guid.NewGuid()).Value
        );

        // Assert
        product.Should().NotBeNull();
        product.DomainEvents.Should().ContainSingle(e => e is ProductCreatedDomainEvent)
            .Which.As<ProductCreatedDomainEvent>().Product.Should().BeSameAs(product);
    }

    [Fact]
    public void Update_ValidData_ShouldUpdatePropertiesAndDispatchProductUpdatedDomainEvent()
    {
        // Arrange 
        var original = Product.Create(
            ProductTitle.Create("OldTitle").Value,
            ProductDescription.Create("OldDesc").Value,
            ProductPrice.Create(100).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId: CategoryId.Create(Guid.NewGuid()).Value
        );
        original.StockQuantity = StockQuantity.Create(5).Value;

        var newTitle = ProductTitle.Create("NewTitle").Value;
        var newDesc = ProductDescription.Create("NewDesc").Value;
        var newPrice = ProductPrice.Create(150).Value;
        var newQty = StockQuantity.Create(10).Value;
        var newSeller = SellerId.Create(Guid.NewGuid()).Value;
        var newCategory = Category.Create(
            CategoryName.Create("Test Category").Value,
            CategoryDescription.Create("Test Description").Value
        );

        // Act 
        original.Update(newTitle, newDesc, newPrice, newQty, newSeller, newCategory);

        // Assert 
        original.Title.Should().Be(newTitle);
        original.Description.Should().Be(newDesc);
        original.Price.Should().Be(newPrice);
        original.StockQuantity.Should().Be(newQty);
        original.SellerId.Should().Be(newSeller);
        original.Category.Should().Be(newCategory);

        original.DomainEvents.Should().ContainSingle(e => e is ProductUpdatedDomainEvent)
            .Which.As<ProductUpdatedDomainEvent>().Product.Should().BeSameAs(original);
    }

    [Fact]
    public void AddImage_ImagesUnderLimit_ShouldAddImageToListAndDispatchProductUpdatedDomainEvent()
    {
        // Arrange
        var product = Product.Create(
            ProductTitle.Create("Title").Value,
            ProductDescription.Create("Desc").Value,
            ProductPrice.Create(50).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId: null
        );
        var img = Image.Create(
            fileName: FileName.Create("image.jpg").Value,
            data: ImageData.Create([1, 2, 3, 4]).Value,
            contentType: ImageContentType.PNG
        );

        // Act
        product.AddImage(img);

        // Assert
        product.Images.Should().HaveCount(1);
        product.Images[0].ProductId.Should().Be(product.Id);
        product.Images[0].Id.Should().Be(img.Id);

        product.DomainEvents.Should().Contain(e => e is ProductUpdatedDomainEvent)
            .Which.As<ProductUpdatedDomainEvent>().Product.Should().BeSameAs(product);
    }

    [Fact]
    public void AddImages_ImagesUnderLimit_ShouldAddImagesToListAndDispatchProductUpdatedDomainEvent()
    {
        // Arrange
        var product = Product.Create(
            ProductTitle.Create("Title").Value,
            ProductDescription.Create("Desc").Value,
            ProductPrice.Create(50).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId: null
        );
        var img1 = Image.Create(
            fileName: FileName.Create("image1.jpg").Value,
            data: ImageData.Create([1, 2, 3, 4]).Value,
            contentType: ImageContentType.PNG
        );
        var img2 = Image.Create(
            fileName: FileName.Create("image2.jpg").Value,
            data: ImageData.Create([1, 2, 3, 4]).Value,
            contentType: ImageContentType.PNG
        );

        // Act
        product.AddImages([ img1, img2 ]);

        // Assert
        product.Images.Should().HaveCount(2);
        product.Images[0].ProductId.Should().Be(product.Id);
        product.Images[0].Id.Should().Be(img1.Id);
        product.Images[1].ProductId.Should().Be(product.Id);
        product.Images[1].Id.Should().Be(img2.Id);

        product.DomainEvents.Should().Contain(e => e is ProductUpdatedDomainEvent)
            .Which.As<ProductUpdatedDomainEvent>().Product.Should().BeSameAs(product);
    }

    [Fact]
    public void RemoveImage_ImageExists_ShouldRemoveImageAndDispatchProductUpdatedDomainEvent()
    {
        // Arrange
        var product = Product.Create(
            ProductTitle.Create("Title").Value,
            ProductDescription.Create("Desc").Value,
            ProductPrice.Create(50).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId: null
        );
        var imageId = ImageId.Create(Guid.NewGuid()).Value;
        var img = Image.Create(
            fileName: FileName.Create("image.jpg").Value,
            data: ImageData.Create([1, 2, 3, 4]).Value,
            contentType: ImageContentType.PNG
        );
        product.AddImage(img);
        product.ClearDomainEvents();

        // Act
        product.RemoveImage(imageId);

        // Assert
        product.Images.Should().NotContain(i => i.Id == imageId);
        product.DomainEvents.Should().Contain(e => e is ProductUpdatedDomainEvent)
            .Which.As<ProductUpdatedDomainEvent>().Product.Should().BeSameAs(product);
    }

    [Fact]
    public void DecreaseQuantity_LessThanOrEqualStock_ShouldReduceStockCorrectly()
    {
        // Arrange
        var product = Product.Create(
            ProductTitle.Create("T").Value,
            ProductDescription.Create("D").Value,
            ProductPrice.Create(20).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId: null
        );
        product.StockQuantity = StockQuantity.Create(5).Value;

        // Act
        product.DecreaseQuantity(3);

        // Assert
        product.StockQuantity.Value.Should().Be(2);
    }

    [Fact]
    public void DecreaseQuantity_QuantityGreaterThanStock_ShouldThrowArgumentException()
    {
        // Arrange
        var product = Product.Create(
            ProductTitle.Create("T").Value,
            ProductDescription.Create("D").Value,
            ProductPrice.Create(20).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            categoryId: null
        );
        product.StockQuantity = StockQuantity.Create(2).Value;

        // Act 
        Action act = () => product.DecreaseQuantity(5);

        // Assert 
        act.Should().Throw<ArgumentException>()
            .WithMessage("*greater than the quantity*");
    }
}