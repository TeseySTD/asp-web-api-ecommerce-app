using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Ordering.Application.UseCases.Orders.EventHandlers.Integration;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events;
using Shared.Messaging.Events.Order;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.EventHandlers.Integration;

public class ApprovedOrderEventHandlerTest : IntegrationTest
{
    public ApprovedOrderEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _logger = Substitute.For<ILogger<IntegrationEventHandler<ApprovedOrderEvent>>>();
        _consumeContext = Substitute.For<ConsumeContext<ApprovedOrderEvent>>();
    }

    private readonly ILogger<IntegrationEventHandler<ApprovedOrderEvent>> _logger;
    private readonly ConsumeContext<ApprovedOrderEvent> _consumeContext;

    private Order CreateTestOrder(Guid orderId) => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        [],
        OrderId.Create(orderId).Value
    );

    private Product CreateTestProduct(Guid productId) => Product.Create(
        ProductId.Create(productId).Value,
        ProductTitle.Create("Test Product").Value,
        ProductDescription.Create("Test Description").Value
    );

    private ApprovedOrderEvent GenerateEvent(Guid orderId, Guid productId) => new
    (
        OrderId: orderId,
        OrderItemsDtos: new List<OrderItemApprovedDto>
        {
            new
            (
                Id: productId,
                ProductTitle: "Test Product",
                ProductDescription: "Test Description",
                Quantity: 2,
                UnitPrice: 10.50m
            )
        }
    );

    [Fact]
    public async Task WhenHandle_WithExistingProducts_ThenApproveOrderWithOrderItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = CreateTestOrder(orderId);
        var existingProduct = CreateTestProduct(productId);

        ApplicationDbContext.Products.Add(existingProduct);
        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var approvedEvent = GenerateEvent(orderId, productId);
        _consumeContext.Message.Returns(approvedEvent);

        var handler = new ApprovedOrderEventHandler(_logger, ApplicationDbContext);

        // Act
        await handler.Handle(_consumeContext);

        // Assert
        var newOrder = await ApplicationDbContext.Orders.FindAsync(order.Id);
        var products = await ApplicationDbContext.Products.ToListAsync();

        newOrder.Should().NotBeNull();
        newOrder.Status.Should().Be(OrderStatus.InProgress);
        newOrder.OrderItems.Should().ContainSingle(i => i.ProductId == existingProduct.Id);

        products.Should().ContainSingle(i => i.Id == existingProduct.Id);
    }

    [Fact]
    public async Task WhenHandle_WithNonExistingProducts_ThenCreateProductsAndApproveOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var order = CreateTestOrder(orderId);
        var nonExistingProduct = CreateTestProduct(productId);

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var approvedEvent = GenerateEvent(orderId, productId); 

        _consumeContext.Message.Returns(approvedEvent);

        var handler = new ApprovedOrderEventHandler(_logger, ApplicationDbContext);

        // Act
        await handler.Handle(_consumeContext);

        // Assert
        var newOrder = await ApplicationDbContext.Orders.FindAsync(order.Id);
        var products = await ApplicationDbContext.Products.ToListAsync();

        newOrder.Should().NotBeNull();
        newOrder.Status.Should().Be(OrderStatus.InProgress);
        newOrder.OrderItems.Should().ContainSingle(i => i.ProductId == nonExistingProduct.Id);

        products.Should().ContainSingle(i => i.Id == nonExistingProduct.Id);
    }
}