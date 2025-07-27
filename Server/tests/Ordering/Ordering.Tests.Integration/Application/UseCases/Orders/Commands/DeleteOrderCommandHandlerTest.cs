using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.UseCases.Orders.Commands.DeleteOrder;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Commands;

public class DeleteOrderCommandHandlerTest : IntegrationTest
{
    public DeleteOrderCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task WhenOrderIsNotInDb_ThenReturnsFailure()
    {
        // Arrange
        var orderId = OrderId.Create(Guid.NewGuid()).Value;

        var cmd = new DeleteOrderCommand(orderId);
        var handler = new DeleteOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new DeleteOrderCommandHandler.OrderNotFoundError(orderId.Value));
    }

    [Fact]
    public async Task WhenOrderIsInDb_ThenReturnsSuccess()
    {
        // Arrange
        var order = Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            OrderId.Create(Guid.NewGuid()).Value
        );
        
        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);
        
        var cmd = new DeleteOrderCommand(order.Id);
        var handler = new DeleteOrderCommandHandler(ApplicationDbContext);
        
        // Act
        var result = await handler.Handle(cmd, default);
        
        // Assert
        Assert.True(result.IsSuccess);
        var isOrderInDb = await ApplicationDbContext.Orders.AnyAsync(o => o.Id == order.Id);
        isOrderInDb.Should().BeFalse();
    }
}