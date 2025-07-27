using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ordering.Application.UseCases.Orders.EventHandlers.Integration;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.EventHandlers.Integration;
public class ProductUpdatedEventHandlerTest : IntegrationTest
    {
        public ProductUpdatedEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture) { }

        [Fact]
        public async Task WhenHandlerHasBeenCalled_ProductIsUpdated()
        {
            // Arrange
            var productIdGuid = Guid.NewGuid();
            var original = Product.Create(
                ProductId.Create(productIdGuid).Value,
                ProductTitle.Create("Old Title").Value,
                ProductDescription.Create("Old Description").Value
            );
            
            ApplicationDbContext.Products.Add(original);
            await ApplicationDbContext.SaveChangesAsync(CancellationToken.None);

            var updatedTitle = "New Title";
            var updatedDescription = "New Description";
            var updateEvent = new ProductUpdatedEvent(
                productIdGuid,
                updatedTitle,
                updatedDescription,
                10,
                new ProductUpdatedEventCategory(Guid.NewGuid(), "Test Category"),
                []
            );
            var consumeContext = Substitute.For<ConsumeContext<ProductUpdatedEvent>>();
            consumeContext.Message.Returns(updateEvent);
            var logger = Substitute.For<ILogger<IntegrationEventHandler<ProductUpdatedEvent>>>();
            var handler = new ProductUpdatedEventHandler(logger, ApplicationDbContext);

            // Act
            await handler.Handle(consumeContext);

            // Assert
            var updated = await ApplicationDbContext.Products.FindAsync(ProductId.Create(productIdGuid).Value);
            updated.Should().NotBeNull();
            updated.Title.Value.Should().Be(updatedTitle);
            updated.Description.Value.Should().Be(updatedDescription);
        }
    }
