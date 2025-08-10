using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Ordering.Application.Dto.Order;
using Ordering.Application.UseCases.Orders.Commands.CreateOrder;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
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

    private (Guid ProductId, uint Quantity) CreateTestOrderItemDto(Guid productId, uint quantity) =>
        (productId, quantity);

    private OrderWriteDto CreateTestOrderWriteDto(IEnumerable<(Guid, uint)> items) => new(
        UserId: Guid.NewGuid(),
        items,
        (cardName: "John Doe", cardNumber: "4111111111111111", expiration: "12/25", cvv: "123", paymentMethod: "Visa"),
        (addressLine: "456 Oak Rd", country: "USA", state: "CA", zipCode: "12345")
    );

    [Fact]
    public async Task Handle_OrderItemIsNotUnique_ReturnsOrderItemIsNotUniqueError()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var dto = CreateTestOrderWriteDto(
            [
                CreateTestOrderItemDto(productId, 3),
                CreateTestOrderItemDto(productId, 4)
            ]
        );
        var command = new CreateOrderCommand(dto);
        var handler = new CreateOrderCommandHandler(ApplicationDbContext, _publishEndpointMock);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new CreateOrderCommandHandler.OrderItemIsNotUniqueError());
    }

    [Fact]
    public async Task Handle_ProductsInOrderNotInDb_ReturnsProductsNotFound()
    {
        // Arrange
        var nonExistingProductId1 = Guid.NewGuid();
        var nonExistingProductId2 = Guid.NewGuid();

        var dto = CreateTestOrderWriteDto(
            [
                CreateTestOrderItemDto(nonExistingProductId1, 3),
                CreateTestOrderItemDto(nonExistingProductId2, 4)
            ]
        );
        var command = new CreateOrderCommand(dto);
        var handler = new CreateOrderCommandHandler(ApplicationDbContext, _publishEndpointMock);

        // Act
        var result = await handler.Handle(command, default);

        // Assert
        Assert.True(result.IsFailure);
        var expectedError =
            new CreateOrderCommandHandler.ProductsNotFound([nonExistingProductId1, nonExistingProductId2]);
        result.Errors.Should().ContainSingle(e => e == expectedError);
    }

    [Fact]
    public async Task Handle_DataIsValid_ReturnsSuccessResultCreateOrderAndPublishEndpoint()
    {
        // Arrange
        var product1 = Product.Create(
            id: ProductId.Create(Guid.NewGuid()).Value,
            title: ProductTitle.Create("Test #1").Value,
            description: ProductDescription.Create("Test Description #1").Value
        );
        var product2 = Product.Create(
            id: ProductId.Create(Guid.NewGuid()).Value,
            title: ProductTitle.Create("Test #2").Value,
            description: ProductDescription.Create("Test Description #2").Value
        );

        ApplicationDbContext.Products.AddRange(product1, product2);
        await ApplicationDbContext.SaveChangesAsync();

        var orderDto = CreateTestOrderWriteDto(
            [
                CreateTestOrderItemDto(product1.Id.Value, 3),
                CreateTestOrderItemDto(product2.Id.Value, 4)
            ]
        );

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