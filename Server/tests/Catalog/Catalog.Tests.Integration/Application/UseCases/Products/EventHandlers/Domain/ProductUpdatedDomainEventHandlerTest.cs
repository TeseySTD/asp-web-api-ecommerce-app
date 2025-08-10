using Catalog.Application.Common.Interfaces;
using Catalog.Application.UseCases.Products.EventHandlers.Domain;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.Events;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Messaging.Events.Product;

namespace Catalog.Tests.Integration.Application.UseCases.Products.EventHandlers.Domain;

public class ProductUpdatedDomainEventHandlerTest : IntegrationTest
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductUpdatedDomainEventHandler> _logger;
    private readonly IImageUrlGenerator _imageUrlGenerator;

    public ProductUpdatedDomainEventHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<ProductUpdatedDomainEventHandler>>();
        _imageUrlGenerator = Substitute.For<IImageUrlGenerator>();
        _imageUrlGenerator.GenerateUrl(Arg.Any<ImageId>()).Returns(i => $"http://img/{i.Arg<ImageId>().Value}");
    }

    [Fact]
    public async Task Handle_ProductUpdatedEventRaised_ShouldPublishMessageWithCorrectData()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = Category.Create(
            CategoryId.Create(categoryId).Value,
            CategoryName.Create("Category").Value,
            CategoryDescription.Create("Description").Value
        );
        var prodId = Guid.NewGuid();
        var product = Product.Create(
            ProductId.Create(prodId).Value,
            ProductTitle.Create("Title").Value,
            ProductDescription.Create("Description").Value,
            ProductPrice.Create(10m).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            category.Id
        );

        // Update to set nav proprerty 'Category' and quantity
        product.Update(product.Title, product.Description, product.Price, StockQuantity.Create(5).Value, product.SellerId, category);
        var imageId = Guid.NewGuid();
        var image = Image.Create(
            ImageId.Create(imageId).Value,
            FileName.Create("FileName").Value,
            ImageData.Create([1]).Value,
            ImageContentType.PNG
        );
        product.AddImage(image);

        var domainEvent = new ProductUpdatedDomainEvent(product);
        var handler = new ProductUpdatedDomainEventHandler(
            _logger,
            _publishEndpoint,
            _imageUrlGenerator
        );

        // Act
        await handler.Handle(domainEvent, default);

        // Assert
        var expectedUrl = "http://img/" + imageId;
        _imageUrlGenerator.Received(1).GenerateUrl(ImageId.Create(imageId).Value);

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<ProductUpdatedEvent>(e =>
                e.ProductId == prodId &&
                e.Title == product.Title.Value &&
                e.Description == product.Description.Value &&
                e.Price == product.Price.Value &&
                e.ImageUrls.Single() == expectedUrl &&
                e.Category!.CategoryId == categoryId
            )
        );
    }
}