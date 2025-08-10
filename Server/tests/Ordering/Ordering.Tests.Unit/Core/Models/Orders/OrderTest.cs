using FluentAssertions;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Orders.Events;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products.ValueObjects;

namespace Ordering.Tests.Unit.Core.Models.Orders;

public class OrderTest
{
    private Order CreateTestOrder() => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        OrderId.Create(Guid.NewGuid()).Value
    );

    [Fact]
    public void Create_ValidData_ReturnsOrderThatIsPendingAndDispatchOrderCreatedDomainEvent()
    {
        // Act
        var order = CreateTestOrder();
        
        // Assert
        order.Status.Should().Be(OrderStatus.NotStarted);
        order.DomainEvents.Should().HaveCount(1);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCreatedDomainEvent);
    }
    
    [Fact]
    public void AddOrderItem_ValidData_ShouldAddOrderItem()
    {
        // Arrange
        var order = CreateTestOrder();
        var orderItem = OrderItem.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            order.Id,
            OrderItemQuantity.Create(10).Value,
            OrderItemPrice.Create(10).Value
        );

        // Act
        order.AddOrderItem(orderItem);

        // Assert
        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.Should().ContainSingle(o => o == orderItem);
    }

    [Fact]
    public void Update_ValidData_ShouldUpdatePropertiesAndDispatchOrderUpdatedDomainEvent()
    {
        // Arrange
        var order = CreateTestOrder();
        var newPayment = Payment.Create("Jane Doe", "4111111111111111", "12/25", "123", "Visa").Value;
        var newAddress = Address.Create("461 Oak Rd", "USA", "CA", "12344").Value;

        // Act
        order.Update(newPayment, newAddress);

        // Assert
        order.Payment.Should().Be(newPayment);
        order.DestinationAddress.Should().Be(newAddress);

        order.DomainEvents.Should().HaveCount(2); // Also has a created event
        order.DomainEvents.Should().ContainSingle(e => e is OrderUpdatedDomainEvent);
    }

    [Fact]
    public void Approve_ValidItems_ShouldChangeOrderState()
    {
        // Assert
        var order = CreateTestOrder();
        var orderItem = OrderItem.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            order.Id,
            OrderItemQuantity.Create(10).Value,
            OrderItemPrice.Create(10).Value
        );

        // Act
        order.Approve([orderItem]);

        // Assert
        order.Status.Should().Be(OrderStatus.InProgress);

        order.OrderItems.Should().HaveCount(1);
        order.OrderItems.Should().ContainSingle(o => o == orderItem);
    }

    [Fact]
    public void Cancel_Called_ShouldChangeOrderState()
    {
        // Assert
        var order = CreateTestOrder();

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Complete_Called_ShouldChangeOrderState()
    {
        // Assert
        var order = CreateTestOrder();

        // Act
        order.Complete();

        // Assert
        order.Status.Should().Be(OrderStatus.Completed);
    }
}