using System.Globalization;
using FluentAssertions;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Queries.GetOrderById;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Core.Auth;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Queries;

public class GetOrderByIdQueryHandlerTest : IntegrationTest
{
    public GetOrderByIdQueryHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    private Order CreateTestOrder() => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        [],
        OrderId.Create(Guid.NewGuid()).Value
    );

    [Fact]
    public async Task Handle_OrderIsNotInDb_ReturnsOrderNotFoundError()
    {
        // Arrange
        var orderId = OrderId.Create(Guid.NewGuid()).Value;
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;

        var query = new GetOrderByIdQuery(orderId, customerId, UserRole.Default);
        var handler = new GetOrderByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new GetOrderByIdQueryHandler.OrderNotFoundError(orderId.Value));
    }

    [Fact]
    public async Task Handle_CustomerIsNotOrderOwner_ReturnsCustomerMismatchError()
    {
        // Arrange
        var order = CreateTestOrder();
        var notOwnerId = CustomerId.Create(Guid.NewGuid()).Value;

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var query = new GetOrderByIdQuery(order.Id, notOwnerId, UserRole.Default);
        var handler = new GetOrderByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is GetOrderByIdQueryHandler.CustomerMismatchError);
    }

    [Fact]
    public async Task Handle_OrderIsInDb_ReturnsSuccess()
    {
        // Arrange
        var order = CreateTestOrder();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var query = new GetOrderByIdQuery(order.Id, order.CustomerId, UserRole.Default);
        var handler = new GetOrderByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);

        result.Value.Should().Satisfy<OrderReadDto>(dto =>
            {
                dto.OrderId.Should().Be(order.Id.Value);
                dto.CustomerId.Should().Be(order.CustomerId.Value);
                dto.OrderDate.Should().Be(order.OrderDate.ToString(CultureInfo.InvariantCulture));
                dto.Status.Should().Be(order.Status.ToString());
                dto.CardName.Should().Be(order.Payment.CardName);
                dto.ShortCardNumber.Should().Be(order.Payment.CardNumber.Substring(0, 3));
                dto.Address.Should().Be(order.DestinationAddress.AddressLine);
                dto.TotalPrice.Should().Be(order.TotalPrice);
                order.OrderItems.Select(oi =>
                    new OrderReadItemDto(
                        oi.Product.Id.Value,
                        oi.Product.Title.Value,
                        oi.Product.Description.Value
                    )
                ).SequenceEqual(dto.Products).Should().BeTrue();
            }
        );
    }

    [Fact]
    public async Task Handle_OrderInDbCustomerIsAdminAndNotOrderOwner_ReturnsSuccess()
    {
        // Arrange
        var order = CreateTestOrder();
        var notOwnerId = CustomerId.Create(Guid.NewGuid()).Value;

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var query = new GetOrderByIdQuery(order.Id, notOwnerId, UserRole.Admin);
        var handler = new GetOrderByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
    }
}