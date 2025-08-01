using Basket.API.Application.UseCases.Cart.Commands.DeleteCart;
using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Basket.Tests.Unit.Application.UseCases.Cart.Commands;

public class DeleteCartCommandHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly DeleteCartCommandHandler _handler;

    public DeleteCartCommandHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new DeleteCartCommandHandler(_cartRepository);
    }

    [Fact]
    public async Task WhenCartIsNotFound_ThenReturnsFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cmd = new DeleteCartCommand(userId);

        var error = new ICartRepository.CartWithUserIdNotFoundError(userId);
        _cartRepository.DeleteCart(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        var result = await _handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e is ICartRepository.CartWithUserIdNotFoundError);
    }

    [Fact]
    public async Task WhenDataIsValid_ThenReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cmd = new DeleteCartCommand(userId);

        _cartRepository.DeleteCart(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsSuccess);
        await _cartRepository.Received(1).DeleteCart(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Any<CancellationToken>());
    }
}