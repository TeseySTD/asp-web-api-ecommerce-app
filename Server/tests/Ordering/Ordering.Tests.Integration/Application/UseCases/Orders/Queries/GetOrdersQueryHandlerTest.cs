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

    private List<Order> GetTestListOrders() =>
    [
        Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value
        ),
        Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("Jane Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            OrderId.Create(Guid.NewGuid()).Value
        ),
        Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("Somebody One", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            OrderId.Create(Guid.NewGuid()).Value
        )
    ];

    [Fact]
    public async Task WhenNoOrdersExist_ThenReturnsFailure()
    {
        // Arrange
        var query = new GetOrdersQuery(new PaginationRequest());
        var handler = new GetOrdersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new GetOrdersQueryHandler.OrdersNotFoundError());
    }

    [Fact]
    public async Task WhenRequestIsOutOfRange_ThenReturnsFailure()
    {
        // Arrange
        var orders = GetTestListOrders();

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync(default);

        var query = new GetOrdersQuery(new PaginationRequest(PageIndex: 1, PageSize: orders.Count));
        var handler = new GetOrdersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new GetOrdersQueryHandler.OrdersNotFoundError());
    }

    [Fact]
    public async Task WhenRequestIsValid_ThenReturnsSuccess()
    {
        // Arrange
        var orders = GetTestListOrders();

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync(default);

        var query = new GetOrdersQuery(new PaginationRequest(PageIndex: 0, PageSize: orders.Count - 1));
        var handler = new GetOrdersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);
        result.Value.Data.Should().HaveCount(orders.Count - 1);
        result.Value.Data.Should().AllSatisfy(dto =>
        {
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
}