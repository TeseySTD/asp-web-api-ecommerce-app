using Basket.API.Application.UseCases.Cart.Commands.RemoveProduct;
using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Basket.Tests.Unit.Application.UseCases.Cart.Commands;

public class RemoveProductCommandHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly RemoveProductCommandHandler _handler;

    public RemoveProductCommandHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new RemoveProductCommandHandler(_cartRepository);
    }

    [Fact]
    public async Task Handle_NonExistentUserId_ReturnsCartWithUserIdNotFoundError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var command = new RemoveProductCommand(userId, productId);
        var error = new ICartRepository.CartWithUserIdNotFoundError(userId);
        _cartRepository.RemoveProductFromCart(
                Arg.Is<UserId>(u => u.Value == userId),
                Arg.Is<ProductId>(p => p.Value == productId),
                Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.CartWithUserIdNotFoundError);
        await _cartRepository.Received(1).RemoveProductFromCart(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Is<ProductId>(p => p.Value == productId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductIdThatNotInCart_ReturnsProductInCartNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var command = new RemoveProductCommand(userId, productId);
        var error = new ICartRepository.ProductInCartNotFound(productId);
        _cartRepository.RemoveProductFromCart(
                Arg.Any<UserId>(), Arg.Any<ProductId>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(error));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.ProductInCartNotFound);
        await _cartRepository.Received(1).RemoveProductFromCart(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Is<ProductId>(p => p.Value == productId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var command = new RemoveProductCommand(userId, productId);
        _cartRepository.RemoveProductFromCart(
                Arg.Any<UserId>(), Arg.Any<ProductId>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _cartRepository.Received(1).RemoveProductFromCart(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Is<ProductId>(p => p.Value == productId),
            Arg.Any<CancellationToken>());
    }
}