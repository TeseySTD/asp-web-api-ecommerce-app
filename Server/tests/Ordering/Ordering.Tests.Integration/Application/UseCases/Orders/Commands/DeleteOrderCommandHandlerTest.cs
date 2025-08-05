using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.UseCases.Orders.Commands.DeleteOrder;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Core.Auth;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Commands;

public class DeleteOrderCommandHandlerTest : IntegrationTest
{
    public DeleteOrderCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    private Order CreateTestOrder(CustomerId? customerId = null) => Order.Create(
        customerId ?? CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        OrderId.Create(Guid.NewGuid()).Value
    );

    [Fact]
    public async Task WhenCustomerIsNotOrderOwner_TherReturnsFailure()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;
        var order = CreateTestOrder();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteOrderCommand(order.Id, customerId, UserRole.Default);
        var handler = new DeleteOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is DeleteOrderCommandHandler.CustomerMismatchError);
    }

    [Fact]
    public async Task WhenOrderIsNotInDb_ThenReturnsFailure()
    {
        // Arrange
        var orderId = OrderId.Create(Guid.NewGuid()).Value;
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;

        var cmd = new DeleteOrderCommand(orderId, customerId, UserRole.Default);
        var handler = new DeleteOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new DeleteOrderCommandHandler.OrderNotFoundError(orderId.Value));
    }

    [Fact]
    public async Task WhenOrderIsInDbAndCustomerIdIsCorrect_ThenReturnsSuccess()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;
        var order = CreateTestOrder(customerId);

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteOrderCommand(order.Id, customerId, UserRole.Default);
        var handler = new DeleteOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsSuccess);
        var isOrderInDb = await ApplicationDbContext.Orders.AnyAsync(o => o.Id == order.Id);
        isOrderInDb.Should().BeFalse();
    }

    [Fact]
    public async Task WhenOrderInDbCustomerIsAdminAndNotOrderOwner_ThenReturnsSuccess()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;
        var order = CreateTestOrder();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new DeleteOrderCommand(order.Id, customerId, UserRole.Admin);
        var handler = new DeleteOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsSuccess);
        var isOrderInDb = await ApplicationDbContext.Orders.AnyAsync(o => o.Id == order.Id);
        isOrderInDb.Should().BeFalse();
    }
}