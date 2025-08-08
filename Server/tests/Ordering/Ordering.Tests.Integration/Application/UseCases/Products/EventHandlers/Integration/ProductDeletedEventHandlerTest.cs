using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ordering.Application.UseCases.Products.EventHandlers.Integration;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.Entities;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Product;

namespace Ordering.Tests.Integration.Application.UseCases.Products.EventHandlers.Integration;

public class ProductDeletedEventHandlerTest : IntegrationTest
{
    public ProductDeletedEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    private OrderItem CreateTestOrderItem(ProductId productId, OrderId orderId) => OrderItem.Create(
        productId,
        orderId,
        OrderItemQuantity.Create(1).Value,
        OrderItemPrice.Create(10).Value
    );

    [Fact]
    public async Task WhenHandlerHasBeenCalled_ThenProductIsDeletedAndOrdersWithItAreCancelled()
    {
        // Arrange
        var productToDelete = Product.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("Test Product1").Value,
            ProductDescription.Create("Test Description1").Value
        );
        var productToSave = Product.Create(
            ProductId.Create(Guid.NewGuid()).Value,
            ProductTitle.Create("Test Product2").Value,
            ProductDescription.Create("Test Description2").Value
        );

        List<Order> testOrders =
        [
            Order.Create(
                CustomerId.Create(Guid.NewGuid()).Value,
                Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
                Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
                OrderId.Create(Guid.NewGuid()).Value
            ),
            Order.Create(
                CustomerId.Create(Guid.NewGuid()).Value,
                Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
                Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
                OrderId.Create(Guid.NewGuid()).Value
            ),
            Order.Create(
                CustomerId.Create(Guid.NewGuid()).Value,
                Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
                Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
                OrderId.Create(Guid.NewGuid()).Value
            ),
        ];

        testOrders[0].AddOrderItem(CreateTestOrderItem(productToDelete.Id, testOrders[0].Id));
        testOrders[1].AddOrderItem(CreateTestOrderItem(productToSave.Id, testOrders[1].Id));
        testOrders[2].AddOrderItem(CreateTestOrderItem(productToDelete.Id, testOrders[2].Id));
        testOrders[2].AddOrderItem(CreateTestOrderItem(productToSave.Id, testOrders[2].Id));

        ApplicationDbContext.Products.AddRange(productToDelete, productToSave);
        ApplicationDbContext.Orders.AddRange(testOrders);
        await ApplicationDbContext.SaveChangesAsync(default);

        var deletedEvent = new ProductDeletedEvent(productToDelete.Id.Value);
        var consumeContext = Substitute.For<ConsumeContext<ProductDeletedEvent>>();
        consumeContext.Message.Returns(deletedEvent);
        var logger = Substitute.For<ILogger<IntegrationEventHandler<ProductDeletedEvent>>>();
        var handler = new ProductDeletedEventHandler(logger, ApplicationDbContext);

        // Act
        await handler.Handle(consumeContext);
        
        // Assert
        ApplicationDbContext.ChangeTracker.Clear();
        var deletedProduct = await ApplicationDbContext.Products.FindAsync(productToDelete.Id);
        deletedProduct.Should().BeNull();
        
        var firstOrder = await ApplicationDbContext.Orders.FindAsync(testOrders[0].Id);
        var secondOrder = await ApplicationDbContext.Orders.FindAsync(testOrders[1].Id);
        var thirdOrder = await ApplicationDbContext.Orders.FindAsync(testOrders[2].Id);
        firstOrder!.Status.Should().Be(OrderStatus.Cancelled);
        secondOrder!.Status.Should().Be(OrderStatus.NotStarted);
        thirdOrder!.Status.Should().Be(OrderStatus.Cancelled);
    }
}