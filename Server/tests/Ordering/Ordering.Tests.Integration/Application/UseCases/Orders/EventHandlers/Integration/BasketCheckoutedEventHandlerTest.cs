using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ordering.Application.UseCases.Orders.Commands.CreateOrder;
using Ordering.Application.UseCases.Orders.EventHandlers.Integration;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Basket;
using Shared.Messaging.Events.Order;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.EventHandlers.Integration;

public class BasketCheckoutedEventHandlerTest : IntegrationTest
{
    public BasketCheckoutedEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task Handle_HasBeenCalled_CreatesOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var payment = new BasketCheckoutedEventPayment(
            "John Doe",
            "4111111111111111",
            "01/26",
            "323",
            "Visa"
        );
        var addrress = new BasketCheckoutedEventDestinationAddress(
            "123 Main Street",
            "USA",
            "",
            "12345"
        );
        IEnumerable<ProductWithQuantityDto> products =
        [
            new (Guid.NewGuid(), 10),
            new (Guid.NewGuid(), 9),
            new (Guid.NewGuid(), 8)
        ];

        var checkoutEvent = new BasketCheckoutedEvent(
            userId,
            products,
            payment,
            addrress
        );
        
        var consumeContext = Substitute.For<ConsumeContext<BasketCheckoutedEvent>>();
        consumeContext.Message.Returns(checkoutEvent);
        var sender = Substitute.For<ISender>();
        var logger = Substitute.For<ILogger<IntegrationEventHandler<BasketCheckoutedEvent>>>();

        var handler = new BasketCheckoutedEventHandler(logger, sender);
        
        // Act
        await handler.Handle(consumeContext);
        
        // Assert
        await sender.Received(1).Send(Arg.Any<CreateOrderCommand>());
    }
}