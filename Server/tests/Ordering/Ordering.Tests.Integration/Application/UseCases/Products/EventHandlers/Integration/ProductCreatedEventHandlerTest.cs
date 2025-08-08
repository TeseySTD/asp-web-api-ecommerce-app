using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ordering.Application.UseCases.Products.EventHandlers.Integration;
using Ordering.Core.Models.Products.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Ordering.Tests.Integration.Application.UseCases.Products.EventHandlers.Integration;

public class ProductCreatedEventHandlerTest : IntegrationTest
{
    public ProductCreatedEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }
    [Fact]
    public async Task WhenHandlerHasBeenCalled_ThenProductIsAddedToDatabase()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var title = "Title";
        var description = "Description";
        var createdEvent = new ProductCreatedEvent(
            productId,
            title,
            description
        );
        
        var consumeContext = Substitute.For<ConsumeContext<ProductCreatedEvent>>();
        consumeContext.Message.Returns(createdEvent);
        var logger = Substitute.For<ILogger<IntegrationEventHandler<ProductCreatedEvent>>>();
        var handler = new ProductCreatedEventHandler(logger, ApplicationDbContext);

        // Act
        await handler.Handle(consumeContext);

        // Assert
        var product = await ApplicationDbContext.Products.FirstOrDefaultAsync(p => p.Id == ProductId.Create(productId).Value);
        product.Should().NotBeNull();
        product.Title.Value.Should().Be(title);
        product.Description.Value.Should().Be(description);
    }
}