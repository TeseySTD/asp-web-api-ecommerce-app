using Catalog.Application.UseCases.Products.EventHandlers.Integration;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Core.Models.Products;
using Catalog.Core.Models.Products.ValueObjects;
using Catalog.Tests.Integration.Common;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Messaging.Events.Order;
using Shared.Messaging.Messages;
using Shared.Messaging.Messages.Order;

namespace Catalog.Tests.Integration.Application.UseCases.Products.EventHandlers.Integration;

public class ReserveProductsMessageHandlerTest : IntegrationTest
{
    private readonly IDistributedCache _cache;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<IntegrationMessageHandler<ReserveProductsMessage>> _logger;

    public ReserveProductsMessageHandlerTest(DatabaseFixture fixture)
        : base(fixture)
    {
        _cache = Substitute.For<IDistributedCache>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<IntegrationMessageHandler<ReserveProductsMessage>>>();
    }

    private Product CreateTestProduct(Guid productId) => Product.Create(
        ProductId.Create(productId).Value,
        ProductTitle.Create("Title").Value,
        ProductDescription.Create("Description").Value,
        ProductPrice.Create(5m).Value,
        SellerId.Create(Guid.NewGuid()).Value,
        null
    );

    [Fact]
    public async Task WhenMissingProducts_ThenPublishesFailedEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var message = new ReserveProductsMessage(
            OrderId: orderId,
            Products: [new ProductWithQuantityDto(Guid.NewGuid(), 1)]
        );
        var context = Substitute.For<ConsumeContext<ReserveProductsMessage>>();
        context.Message.Returns(message);

        var handler = new ReserveProductsMessageHandler(
            logger: _logger,
            dbContext: ApplicationDbContext,
            publishEndpoint: _publishEndpoint,
            cache: _cache
        );

        // Act
        await handler.Handle(context);

        // Assert
        var msgStr = ReserveProductsMessageHandler.GenerateMissingProductMessage([message.Products[0].ProductId]);
        await _publishEndpoint.Received(1).Publish(
            Arg.Is<ReservationProductsFailedEvent>(e =>
                e.OrderId == orderId && e.Reason == msgStr)
        );
    }

    [Fact]
    public async Task WhenInsufficientQuantity_ThenPublishesFailedEvent()
    {
        // Arrange
        var prodId = Guid.NewGuid();
        var product = CreateTestProduct(prodId);
        product.StockQuantity = StockQuantity.Create(1).Value;

        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var orderId = Guid.NewGuid();
        var message = new ReserveProductsMessage(
            OrderId: orderId,
            Products: [new ProductWithQuantityDto(prodId, 2)]
        );
        var context = Substitute.For<ConsumeContext<ReserveProductsMessage>>();
        context.Message.Returns(message);

        var handler = new ReserveProductsMessageHandler(
            logger: _logger,
            dbContext: ApplicationDbContext,
            publishEndpoint: _publishEndpoint,
            cache: _cache
        );

        // Act
        await handler.Handle(context);

        // Assert
        var msgStr = ReserveProductsMessageHandler.GenerateIssuficientQuantityMessage(prodId);
        await _publishEndpoint.Received(1).Publish(
            Arg.Is<ReservationProductsFailedEvent>(e =>
                e.OrderId == orderId && e.Reason == msgStr
            )
        );
    }

    [Fact]
    public async Task WhenValidReservation_ThenUpdatesQuantities_AndPublishesReservedEvent()
    {
        // Arrange
        var prodId = Guid.NewGuid();
        var product = CreateTestProduct(prodId);
        product.StockQuantity = StockQuantity.Create(10).Value;
        ApplicationDbContext.Products.Add(product);
        await ApplicationDbContext.SaveChangesAsync(default);

        var orderId = Guid.NewGuid();
        uint qty = 3;
        var message = new ReserveProductsMessage(
            OrderId: orderId,
            Products: [new ProductWithQuantityDto(prodId, qty)]
        );
        var context = Substitute.For<ConsumeContext<ReserveProductsMessage>>();
        context.Message.Returns(message);

        var handler = new ReserveProductsMessageHandler(
            logger: _logger,
            dbContext: ApplicationDbContext,
            publishEndpoint: _publishEndpoint,
            cache: _cache
        );

        // Act
        await handler.Handle(context);

        // Assert
        var updated = await ApplicationDbContext.Products.FindAsync(ProductId.Create(prodId).Value);
        updated!.StockQuantity.Value.Should().Be(7);
        await _cache.Received(1).RemoveAsync($"product-{prodId}");

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<ReservedProductsEvent>(e =>
                e.OrderId == orderId &&
                e.OrderItemsDtos.Single().Id == prodId &&
                e.OrderItemsDtos.Single().Quantity == qty
            )
        );
    }
}