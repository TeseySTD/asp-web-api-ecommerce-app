using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ordering.Application.UseCases.Orders.EventHandlers.Integration;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.EventHandlers.Integration;
public class CanceledOrderEventHandlerTest : IntegrationTest
    {
        public CanceledOrderEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture) { }

        [Fact]
        public async Task WhenHandlerHasBeenCalled_ThenOrderIsCancelled()
        {
            // Arrange
            var orderIdGuid = Guid.NewGuid();
            var reason = "Test cancellation";
            var canceledEvent = new CanceledOrderEvent(orderIdGuid, reason);

            var orderEntity = Order.Create(
                CustomerId.Create(Guid.NewGuid()).Value,
                Payment.Create("Test", "0000", "01/99", "000", "Visa").Value,
                Address.Create("","","","").Value,
                OrderId.Create(orderIdGuid).Value
            );
            ApplicationDbContext.Orders.Add(orderEntity);
            await ApplicationDbContext.SaveChangesAsync(CancellationToken.None);

            var consumeContext = Substitute.For<ConsumeContext<CanceledOrderEvent>>();
            consumeContext.Message.Returns(canceledEvent);
            var logger = Substitute.For<ILogger<IntegrationEventHandler<CanceledOrderEvent>>>();
            var handler = new CanceledOrderEventHandler(logger, ApplicationDbContext);

            // Act
            await handler.Handle(consumeContext);

            // Assert
            var updatedOrder = await ApplicationDbContext.Orders.FindAsync(OrderId.Create(orderIdGuid).Value);
            updatedOrder.Should().NotBeNull();
            updatedOrder.Status.Should().Be(OrderStatus.Cancelled);
        }
    }
