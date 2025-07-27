using FluentAssertions;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Queries.GetOrderById;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Queries;

public class GetOrderByIdQueryHandlerTest : IntegrationTest
{
    public GetOrderByIdQueryHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task WhenOrderIsNotFound_ThenReturnsFailure()
    {
        // Arrange
        var orderId = OrderId.Create(Guid.NewGuid()).Value;

        var query = new GetOrderByIdQuery(orderId);
        var handler = new GetOrderByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new GetOrderByIdQueryHandler.OrderNotFoundError(orderId.Value));
    }

    [Fact]
    public async Task WhenOrderIsFound_ThenReturnsSuccess()
    {
        // Arrange
        var order = Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value
        );

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var query = new GetOrderByIdQuery(order.Id);
        var handler = new GetOrderByIdQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);

        result.Value.Should().Satisfy<OrderReadDto>(dto =>
            {
                dto.OrderId.Should().Be(order.Id.Value);
                dto.CustomerId.Should().Be(order.CustomerId.Value);
                dto.OrderDate.Should().Be(order.OrderDate.ToString());
                dto.Status.Should().Be(order.Status.ToString());
                dto.CardName.Should().Be( order.Payment.CardName );
                dto.ShortCardNumber.Should().Be(order.Payment.CardNumber.Substring(0, 3));
                dto.Address.Should().Be( order.DestinationAddress.AddressLine );
                dto.TotalPrice.Should().Be( order.TotalPrice );
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
}