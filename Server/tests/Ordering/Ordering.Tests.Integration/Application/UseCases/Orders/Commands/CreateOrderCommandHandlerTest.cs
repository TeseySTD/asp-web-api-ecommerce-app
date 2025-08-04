using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Commands.CreateOrder;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;
using Shared.Messaging.Events.Order;

namespace Ordering.Tests.Integration.Application.UseCases.Orders.Commands;

public class CreateOrderCommandHandlerTest : IntegrationTest
{
    private IPublishEndpoint _publishEndpointMock;

    public CreateOrderCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _publishEndpointMock = Substitute.For<IPublishEndpoint>();
    }

    private Order CreateTestOrder() => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        OrderId.Create(Guid.NewGuid()).Value
    );

    private (Guid ProductId, uint Quantity) CreateTestOrderItemDto(Guid productId, uint quantity) =>
        (productId, quantity);

    private OrderWriteDto CreateTestOrderWriteDto() => new(
        UserId: Guid.NewGuid(),
        [(ProductId: Guid.NewGuid(), Quantity: 3)],
        (cardName: "John Doe", cardNumber: "4111111111111111", expiration: "12/25", cvv: "123", paymentMethod: "Visa"),
        (addressLine: "456 Oak Rd", country: "USA", state: "CA", zipCode: "12345")
    );

    [Fact]
    public async Task WhenOrderItemIsNotUnique_ThenReturnsFailureResult()
    {
        // Arrange
        var order = CreateTestOrder();
        var productId = Guid.NewGuid();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync(default);

        var dto = CreateTestOrderWriteDto() with
        {
            OrderItems = [CreateTestOrderItemDto(productId, 3), CreateTestOrderItemDto(productId, 4)]
        };
        var command = new CreateOrderCommand(dto);
        var handler = new CreateOrderCommandHandler(ApplicationDbContext, _publishEndpointMock);
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CreateOrderCommandHandler.OrderItemIsNotUniqueError());
    }

    [Fact]
    public async Task WhenDataIsCorrect_ThenReturnsSuccessResultCreateOrderAndPublishEndpoint()
    {
        // Arrange
        var orderDto = CreateTestOrderWriteDto();
        
        var command = new CreateOrderCommand(orderDto);
        var handler = new CreateOrderCommandHandler(ApplicationDbContext, _publishEndpointMock);
        
        // Act
        var result = await handler.Handle(command, default);
        
        // Assert
        Assert.True(result.IsSuccess);
        
        var order = await ApplicationDbContext.Orders.FirstAsync();
        Assert.NotNull(order);
        order.Should().Satisfy<Order>(o =>
        {
            o.CustomerId.Value.Should().Be(orderDto.UserId);
            o.Payment.Should().Satisfy<Payment>(p =>
            {
                p.CardName.Should().Be(orderDto.Payment.cardName);
                p.CardNumber.Should().Be(orderDto.Payment.cardNumber);
                p.Expiration.Should().Be(orderDto.Payment.expiration);
                p.CVV.Should().Be(orderDto.Payment.cvv);
                p.PaymentMethod.Should().Be(orderDto.Payment.paymentMethod);
            });
            o.DestinationAddress.Should().Satisfy<Address>(a =>
            {
                a.AddressLine.Should().Be(orderDto.DestinationAddress.addressLine);
                a.Country.Should().Be(orderDto.DestinationAddress.country);
                a.State.Should().Be(orderDto.DestinationAddress.state);
                a.ZipCode.Should().Be(orderDto.DestinationAddress.zipCode);
            });
        });

        await _publishEndpointMock.Received(1).Publish(Arg.Any<OrderMadeEvent>(), Arg.Any<CancellationToken>());
    }
}