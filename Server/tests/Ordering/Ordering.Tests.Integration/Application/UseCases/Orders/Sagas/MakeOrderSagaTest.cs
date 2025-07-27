using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.UseCases.Orders.Sagas;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events.Order;
using Shared.Messaging.Messages.Order;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Sagas;

public class MakeOrderSagaTest : IntegrationTest
{
    public MakeOrderSagaTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    private ServiceProvider _provider = null!;
    private ITestHarness _harness = null!;
    private const int DelayTime = 20;

    public override async Task InitializeAsync()
    {
        _provider = new ServiceCollection()
            .AddMassTransitTestHarness(cfg =>
            {
                cfg.AddSagaStateMachine<MakeOrderSaga, MakeOrderSagaState>()
                    .InMemoryRepository();
            })
            .BuildServiceProvider(true);

        _harness = _provider.GetRequiredService<ITestHarness>();
        await _harness.Start();
    }

    public override async Task DisposeAsync()
    {
        await _harness.Stop();
        await _provider.DisposeAsync();
    }

    [Fact]
    public async Task WhenOrderMade_ThenTransitionsToCheckingCustomerAndPublishCheckCustomerMessage()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var products = new List<ProductWithQuantityDto>
        {
            new(Guid.NewGuid(), 2),
            new(Guid.NewGuid(), 1)
        };

        var orderMadeEvent = new OrderMadeEvent(orderId, customerId, products);

        // Act
        await _harness.Bus.Publish(orderMadeEvent);

        // Assert
        // Verify the message was consumed
        (await _harness.Consumed.Any<OrderMadeEvent>()).Should().BeTrue();

        // Verify CheckCustomerMessage was published
        (await _harness.Published.Any<CheckCustomerMessage>()).Should().BeTrue();

        var publishedMessage = _harness.Published.Select<CheckCustomerMessage>().FirstOrDefault();
        publishedMessage.Should().NotBeNull();
        publishedMessage.Context.Message.OrderId.Should().Be(orderId);
        publishedMessage.Context.Message.CustomerId.Should().Be(customerId);

        // Verify saga state
        var sagaHarness = _harness.GetSagaStateMachineHarness<MakeOrderSaga, MakeOrderSagaState>();
        var saga = sagaHarness.Sagas.Contains(orderId);
        saga.Should().NotBeNull();
        saga.CurrentState.Should().Be("CheckingCustomer");
        saga.ProductWithQuantityDtos.Should().BeEquivalentTo(products);
    }

    [Fact]
    public async Task
        WhenCustomerCheckedInCheckingCustomerState_ThenTransitToReservingProductsAndPublishReserveProductsMessage()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var products = new List<ProductWithQuantityDto>
        {
            new(Guid.NewGuid(), 3)
        };

        // Start saga with OrderMade
        await _harness.Bus.Publish(new OrderMadeEvent(orderId, customerId, products));
        await Task.Delay(DelayTime); // Wait for saga to process

        var checkedCustomerEvent = new CheckedCustomerEvent(orderId);

        // Act
        await _harness.Bus.Publish(checkedCustomerEvent);

        // Assert
        (await _harness.Consumed.Any<CheckedCustomerEvent>()).Should().BeTrue();
        (await _harness.Published.Any<ReserveProductsMessage>()).Should().BeTrue();

        var publishedMessage = _harness.Published.Select<ReserveProductsMessage>().FirstOrDefault();
        publishedMessage.Should().NotBeNull();
        publishedMessage.Context.Message.OrderId.Should().Be(orderId);
        publishedMessage.Context.Message.Products.Should().BeEquivalentTo(products);

        var sagaHarness = _harness.GetSagaStateMachineHarness<MakeOrderSaga, MakeOrderSagaState>();
        var saga = sagaHarness.Sagas.Contains(orderId);
        saga.Should().NotBeNull();
        saga.CurrentState.Should().Be("ReservingProducts");
    }

    [Fact]
    public async Task WhenCustomerCheckingFailedInCheckingCustomerState_ThenFinalizesAndPublishCanceledOrderEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var products = new List<ProductWithQuantityDto>
        {
            new(Guid.NewGuid(), 1)
        };

        // Start saga with OrderMade
        await _harness.Bus.Publish(new OrderMadeEvent(orderId, customerId, products));
        await Task.Delay(DelayTime);

        var checkingCustomerFailedEvent = new CheckingCustomerFailedEvent(orderId);

        // Act
        await _harness.Bus.Publish(checkingCustomerFailedEvent);

        // Assert
        (await _harness.Consumed.Any<CheckingCustomerFailedEvent>()).Should().BeTrue();
        (await _harness.Published.Any<CanceledOrderEvent>()).Should().BeTrue();

        var publishedMessage = _harness.Published.Select<CanceledOrderEvent>().FirstOrDefault();
        publishedMessage.Should().NotBeNull();
        publishedMessage.Context.Message.OrderId.Should().Be(orderId);
        publishedMessage.Context.Message.Reason.Should().Be("Checking customer failed.");

        var sagaHarness = _harness.GetSagaStateMachineHarness<MakeOrderSaga, MakeOrderSagaState>();
        var saga = sagaHarness.Sagas.Contains(orderId);
        saga.Should().NotBeNull();
        saga.CurrentState.Should().Be("Final");
    }

    [Fact]
    public async Task WhenProductsReservedInReservingProductsState_ThenFinalizesAndPublishApprovedOrderEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var products = new List<ProductWithQuantityDto>
        {
            new(Guid.NewGuid(), 2)
        };
        var orderItems = new List<OrderItemApprovedDto>
        {
            new(Guid.NewGuid(), "Product 1", "Product 1 Description", 2, 10.50m)
        };

        // Start saga and transition to ReservingProducts state
        await _harness.Bus.Publish(new OrderMadeEvent(orderId, customerId, products));
        await Task.Delay(DelayTime);
        await _harness.Bus.Publish(new CheckedCustomerEvent(orderId));
        await Task.Delay(DelayTime);

        var reservedProductsEvent = new ReservedProductsEvent(orderId, orderItems);

        // Act
        await _harness.Bus.Publish(reservedProductsEvent);

        // Assert
        (await _harness.Consumed.Any<ReservedProductsEvent>()).Should().BeTrue();
        (await _harness.Published.Any<ApprovedOrderEvent>()).Should().BeTrue();

        var publishedMessage = _harness.Published.Select<ApprovedOrderEvent>().FirstOrDefault();
        publishedMessage.Should().NotBeNull();
        publishedMessage.Context.Message.OrderId.Should().Be(orderId);
        publishedMessage.Context.Message.OrderItemsDtos.Should().BeEquivalentTo(orderItems);

        var sagaHarness = _harness.GetSagaStateMachineHarness<MakeOrderSaga, MakeOrderSagaState>();
        var saga = sagaHarness.Sagas.Contains(orderId);
        saga.Should().NotBeNull();
        saga.CurrentState.Should().Be("Final");
    }

    [Fact]
    public async Task WhenProductsReservationFailedInReservingProductsState_ThenFinalizesAndPublishCanceledOrderEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var products = new List<ProductWithQuantityDto>
        {
            new(Guid.NewGuid(), 5)
        };
        var failureReason = "Insufficient stock";

        // Start saga and transition to ReservingProducts state
        await _harness.Bus.Publish(new OrderMadeEvent(orderId, customerId, products));
        await Task.Delay(DelayTime);
        await _harness.Bus.Publish(new CheckedCustomerEvent(orderId));
        await Task.Delay(DelayTime);

        var reservationFailedEvent = new ReservationProductsFailedEvent(orderId, failureReason);

        // Act
        await _harness.Bus.Publish(reservationFailedEvent);

        // Assert
        (await _harness.Consumed.Any<ReservationProductsFailedEvent>()).Should().BeTrue();
        (await _harness.Published.Any<CanceledOrderEvent>()).Should().BeTrue();

        var publishedMessage = _harness.Published.Select<CanceledOrderEvent>().FirstOrDefault();
        publishedMessage.Should().NotBeNull();
        publishedMessage.Context.Message.OrderId.Should().Be(orderId);
        publishedMessage.Context.Message.Reason.Should().Be(failureReason);

        var sagaHarness = _harness.GetSagaStateMachineHarness<MakeOrderSaga, MakeOrderSagaState>();
        var saga = sagaHarness.Sagas.Contains(orderId);
        saga.Should().NotBeNull();
        saga.CurrentState.Should().Be("Final");
    }

    [Fact]
    public async Task WhenCompleteWorkflow_ThenTransitionsThroughAllStatesCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var products = new List<ProductWithQuantityDto>
        {
            new(Guid.NewGuid(), 1),
            new(Guid.NewGuid(), 3)
        };
        var orderItems = new List<OrderItemApprovedDto>
        {
            new(Guid.NewGuid(), "Product A", "Product A Description", 1, 15.00m),
            new(Guid.NewGuid(), "Product B", "Product B Description", 3, 8.50m)
        };

        var sagaHarness = _harness.GetSagaStateMachineHarness<MakeOrderSaga, MakeOrderSagaState>();

        // Act & Assert - Full workflow
        // Step 1: OrderMade
        await _harness.Bus.Publish(new OrderMadeEvent(orderId, customerId, products));
        await Task.Delay(DelayTime);

        var saga = sagaHarness.Sagas.Contains(orderId);
        saga.CurrentState.Should().Be("CheckingCustomer");
        (await _harness.Published.Any<CheckCustomerMessage>()).Should().BeTrue();

        // Step 2: CustomerChecked
        await _harness.Bus.Publish(new CheckedCustomerEvent(orderId));
        await Task.Delay(DelayTime);

        saga.CurrentState.Should().Be("ReservingProducts");
        (await _harness.Published.Any<ReserveProductsMessage>()).Should().BeTrue();

        // Step 3: ProductsReserved
        await _harness.Bus.Publish(new ReservedProductsEvent(orderId, orderItems));
        await Task.Delay(DelayTime);

        saga.CurrentState.Should().Be("Final");
        (await _harness.Published.Any<ApprovedOrderEvent>()).Should().BeTrue();

        // Verify final published message
        var approvedOrderEvent = _harness.Published.Select<ApprovedOrderEvent>().FirstOrDefault();
        approvedOrderEvent!.Context.Message.OrderId.Should().Be(orderId);
        approvedOrderEvent.Context.Message.OrderItemsDtos.Should().BeEquivalentTo(orderItems);
    }
}