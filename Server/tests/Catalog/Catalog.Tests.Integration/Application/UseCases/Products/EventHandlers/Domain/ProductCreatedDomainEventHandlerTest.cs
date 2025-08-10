using Catalog.Application.UseCases.Products.EventHandlers.Domain;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.Events;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Messaging.Events.Product;

namespace Catalog.Tests.Integration.Application.UseCases.Products.EventHandlers.Domain;

public class ProductCreatedDomainEventHandlerTest : IntegrationTest
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductCreatedDomainEventHandler> _logger;

    public ProductCreatedDomainEventHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<ProductCreatedDomainEventHandler>>();
    }


    [Fact]
    public async Task Handle_ProductCreatedDomainEventRaised_ShouldPublishMessageWithCorrectData()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var title = "Product Title";
        var description = "Product Description";
        var product = Product.Create(
            ProductId.Create(productId).Value,
            ProductTitle.Create(title).Value,
            ProductDescription.Create(description).Value,
            ProductPrice.Create(10m).Value,
            SellerId.Create(Guid.NewGuid()).Value,
            null
        );

        var domainEvent = new ProductCreatedDomainEvent(product);
        var handler = new ProductCreatedDomainEventHandler(
            _logger,
            _publishEndpoint
        );

        // Act
        await handler.Handle(domainEvent, default);
        
        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<ProductCreatedEvent>(
            e => e.ProductId == productId &&
                 e.Title == title &&
                 e.Description == description)
        );
    }
}