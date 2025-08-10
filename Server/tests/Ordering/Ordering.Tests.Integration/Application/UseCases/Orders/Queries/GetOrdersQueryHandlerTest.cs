using System.Globalization;
using FluentAssertions;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Queries.GetOrders;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Core.API;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Queries;

public class GetOrdersQueryHandlerTest : IntegrationTest
{
    public GetOrdersQueryHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    private List<Order> GetTestListOrders(Guid customerId) =>
    [
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("Jane Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.InProgress
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("Somebody One", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.InProgress
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.InProgress
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.Completed
        )
    ];

    [Fact]
    public async Task Handle_NoOrdersExist_ReturnsOrdersNotFoundError()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;
        var query = new GetOrdersQuery(new PaginationRequest(), customerId, null);
        var handler = new GetOrdersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new GetOrdersQueryHandler.OrdersNotFoundError());
    }

    [Fact]
    public async Task Handle_RequestIsOutOfRange_ReturnsOrdersNotFoundError()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;
        var orders = GetTestListOrders(customerId.Value);

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();

        var query = new GetOrdersQuery(new PaginationRequest(PageIndex: 1, PageSize: orders.Count), customerId, null);
        var handler = new GetOrdersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new GetOrdersQueryHandler.OrdersNotFoundError());
    }

    [Fact]
    public async Task Handle_RequestIsValid_ReturnsSuccess()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;
        var orders = GetTestListOrders(customerId.Value);
        var filteredOrders = orders.Where(o => o.Status == OrderStatus.InProgress).ToList();

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();

        var query = new GetOrdersQuery(new PaginationRequest(PageIndex: 0, PageSize: filteredOrders.Count() - 1), customerId, OrderStatus.InProgress);
        var handler = new GetOrdersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Data.Should().HaveCount(filteredOrders.Count() - 1);
        result.Value.Data.Should().AllSatisfy(dto =>
        {
            dto.Status.Should().Be(OrderStatus.InProgress.ToString());
            orders.Should().Contain(o =>
                dto.OrderId == o.Id.Value &&
                dto.CustomerId == o.CustomerId.Value &&
                dto.OrderDate == o.OrderDate.ToString(CultureInfo.InvariantCulture) &&
                dto.Status == o.Status.ToString() &&
                dto.CardName == o.Payment.CardName &&
                dto.ShortCardNumber == o.Payment.CardNumber.Substring(0, 3) &&
                dto.Address == o.DestinationAddress.AddressLine &&
                dto.TotalPrice == o.TotalPrice &&
                o.OrderItems.Select(oi =>
                    new OrderReadItemDto(
                        oi.Product.Id.Value,
                        oi.Product.Title.Value,
                        oi.Product.Description.Value
                    )
                ).SequenceEqual(dto.Products)
            );
        });
    }

    [Fact]
    public async Task Handle_RequestIsValidAndHasOrdersNotWithCustomerId_ReturnsSuccessAndCustomersOrdersOnly()
    {
        // Arrange
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;
        var orders = GetTestListOrders(customerId.Value);
        var notCustomersOrder = Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("Somebody One", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            OrderId.Create(Guid.NewGuid()).Value
        );
        orders.Add(notCustomersOrder);

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();

        var query = new GetOrdersQuery(new PaginationRequest(PageIndex: 0, PageSize: orders.Count), customerId, null);
        var handler = new GetOrdersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Data.Should().HaveCount(orders.Count - 1); // Because one order is not customer`s.
    }
}