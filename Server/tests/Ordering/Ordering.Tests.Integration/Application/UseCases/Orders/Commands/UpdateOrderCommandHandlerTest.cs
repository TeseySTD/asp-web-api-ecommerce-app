using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Commands.UpdateOrder;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Commands;

public class UpdateOrderCommandHandlerTest : IntegrationTest
{
    public UpdateOrderCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    private Order CreateTestOrder(OrderStatus status = OrderStatus.NotStarted) => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        [],
        OrderId.Create(Guid.NewGuid()).Value,
        status
    );

    private OrderUpdateDto CreateTestOrderUpdateDto() => new(
        (cardName: "John Doe", cardNumber: "4111111111111111", expiration: "12/25", cvv: "123", paymentMethod: "Visa"),
        (addressLine: "456 Oak Rd", country: "USA", state: "CA", zipCode: "12345")
    );


    [Fact]
    public async Task WhenOrderIsNotInDb_ThenReturnsFailure()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var cmd = new UpdateOrderCommand(Guid.NewGuid(), orderId, CreateTestOrderUpdateDto());
        var handler = new UpdateOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new UpdateOrderCommandHandler.OrderNotFoundError(orderId));
    }

    [Fact]
    public async Task WhenCustomerIsNotOrderOwner_ThenReturnsFailure()
    {
        // Arrange
        var order = CreateTestOrder();
        var customerId = Guid.NewGuid();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new UpdateOrderCommand(customerId, order.Id.Value, CreateTestOrderUpdateDto());
        var handler = new UpdateOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new UpdateOrderCommandHandler.CustomerMismatchError(customerId));
    }

    [Theory]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed)]
    public async Task WhenOrderStateIsIncorrect_ThenReturnsFailure(OrderStatus orderStatus)
    {
        // Arrange
        var order = CreateTestOrder(orderStatus);
        
        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new UpdateOrderCommand(order.CustomerId.Value, order.Id.Value, CreateTestOrderUpdateDto());
        var handler = new UpdateOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new UpdateOrderCommandHandler.IncorrectOrderStateError());
    }

    [Fact]
    public async Task WhenDataIsCorrect_ThenReturnsSuccess()
    {
        var order = CreateTestOrder();
        var newPayment = (
            cardName: "Jane Doe",
            cardNumber: "4111111111111111",
            expiration: "09/26",
            cvv: "321",
            paymentMethod: "Visa"
        );
        var newAddress = (
            addressLine: "457 Oak Rd",
            country: "UA",
            state: "",
            zipCode: "12345"
        );
        var dto = new OrderUpdateDto(Payment: newPayment, DestinationAddress: newAddress);

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new UpdateOrderCommand(order.CustomerId.Value, order.Id.Value, dto);
        var handler = new UpdateOrderCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsSuccess);

        var newOrder = await ApplicationDbContext.Orders.SingleOrDefaultAsync(o => o.Id == order.Id);
        Assert.NotNull(newOrder);
        newOrder.Payment.Should().Satisfy<Payment>(p =>
        {
            p.CardName.Should().Be(newPayment.cardName);
            p.CardNumber.Should().Be(newPayment.cardNumber);
            p.Expiration.Should().Be(newPayment.expiration);
            p.CVV.Should().Be(newPayment.cvv);
            p.PaymentMethod.Should().Be(newPayment.paymentMethod);
        });
        newOrder.DestinationAddress.Should().Satisfy<Address>(a =>
        {
            a.AddressLine.Should().Be(newAddress.addressLine);
            a.Country.Should().Be(newAddress.country);
            a.State.Should().Be(newAddress.state);
            a.ZipCode.Should().Be(newAddress.zipCode);
        });
    }
}