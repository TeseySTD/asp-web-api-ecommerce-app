using Basket.API.Application.UseCases.Cart.Commands.CheckoutBasket;
using Basket.API.Data.Abstractions;
using Basket.API.Dto.Cart;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;
using MassTransit;
using NSubstitute;
using Shared.Core.Validation.Result;
using Shared.Messaging.Events.Basket;

namespace Basket.Tests.Unit.Application.UseCases.Cart.Commands;

public class CheckoutBasketCommandHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly CheckoutBasketCommandHandler _handler;

    public CheckoutBasketCommandHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _handler = new CheckoutBasketCommandHandler(_cartRepository, _publishEndpoint);
    }

    private  CheckoutBasketDto CreateValidCheckoutBasketDto(Guid userId)
    {
        return new CheckoutBasketDto(
            UserId: userId,
            DestinationAddress: CreateValidAddress(),
            Payment: CreateValidPayment()
        );
    }

    private  (string addressLine, string? country, string? state, string? zipCode) CreateValidAddress()
    {
        return (
            addressLine: "123 Test Street",
            country: "Ukraine",
            state: "Kyiv",
            zipCode: "01001"
        );
    }

    private  (string cardName, string cardNumber, string? expiration, string cvv, string? paymentMethod)
        CreateValidPayment()
    {
        return (
            cardName: "John Doe",
            cardNumber: "1234567890123456",
            expiration: "12/25",
            cvv: "123",
            paymentMethod: "1"
        );
    }

    private static ProductCartItem CreateProductCartItem(Guid productId, uint quantity, decimal price)
    {
        return ProductCartItem.Create(
            ProductId.Create(productId).Value,
            ProductTitle.Create("Test Product").Value,
            StockQuantity.Create(quantity).Value,
            ProductPrice.Create(price).Value,
            ProductCartItemCategory.Create(
                CategoryId.Create(Guid.NewGuid()).Value,
                CategoryName.Create("Test Category").Value
            )
        );
    }

    private static ProductCart CreateProductCart(Guid userId, List<ProductCartItem> items)
    {
        var cart = ProductCart.Create(UserId.From(userId));

        foreach (var item in items)
        {
            cart.AddItem(item);
        }

        return cart;
    }

    [Fact]
    public async Task WhenValidCartWithItems_ThenReturnsSuccessAndPublishesEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var checkoutDto = CreateValidCheckoutBasketDto(userId);
        var command = new CheckoutBasketCommand(checkoutDto);

        var cartItems = new List<ProductCartItem>
        {
            CreateProductCartItem(productId1, 2, 99.99m),
            CreateProductCartItem(productId2, 1, 149.99m)
        };

        var cart = CreateProductCart(userId, cartItems);

        _cartRepository.GetCartByUserId(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductCart>.Success(cart));

        _cartRepository.DeleteCart(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        await _cartRepository.Received(1).GetCartByUserId(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Any<CancellationToken>());

        await _cartRepository.Received(1).DeleteCart(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Any<CancellationToken>());

        await _publishEndpoint.Received(1).Publish(
            Arg.Is<BasketCheckoutedEvent>(e =>
                e.UserId == userId &&
                e.Products.Count() == 2 &&
                e.Products.Any(p => p.ProductId == productId1 && p.ProductQuantity == 2) &&
                e.Products.Any(p => p.ProductId == productId2 && p.ProductQuantity == 1)
            ),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenCartNotFound_ThenReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var checkoutDto = CreateValidCheckoutBasketDto(userId);
        var command = new CheckoutBasketCommand(checkoutDto);

        var cartNotFoundError = new ICartRepository.CartWithUserIdNotFoundError(userId);
        _cartRepository.GetCartByUserId(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductCart>.Failure(cartNotFoundError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is ICartRepository.CartWithUserIdNotFoundError);

        await _cartRepository.DidNotReceive().DeleteCart(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        await _publishEndpoint.DidNotReceive().Publish(Arg.Any<BasketCheckoutedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenDeleteCartFails_ReturnsFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var checkoutDto = CreateValidCheckoutBasketDto(userId);
        var command = new CheckoutBasketCommand(checkoutDto);

        var cart = CreateProductCart(userId, new List<ProductCartItem>
        {
            CreateProductCartItem(Guid.NewGuid(), 1, 50.00m)
        });

        _cartRepository.GetCartByUserId(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Result<ProductCart>.Success(cart));

        var deleteError = new Error("Delete failed", "Cart deletion failed");
        _cartRepository.DeleteCart(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(deleteError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == deleteError);

        await _cartRepository.Received(1).GetCartByUserId(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        await _cartRepository.Received(1).DeleteCart(Arg.Any<UserId>(), Arg.Any<CancellationToken>());
        await _publishEndpoint.DidNotReceive().Publish(Arg.Any<BasketCheckoutedEvent>(), Arg.Any<CancellationToken>());
    }
}