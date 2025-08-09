using Basket.API.Application.UseCases.Cart.Queries.GetCartByUserId;
using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Basket.Tests.Unit.Application.UseCases.Cart.Queries;

public class GetCartByUserIdQueryHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly GetCartByUserIdQueryHandler _handler;

    public GetCartByUserIdQueryHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new GetCartByUserIdQueryHandler(_cartRepository);
    }

    [Fact]
    public async Task Handle_CartExists_ReturnsSuccessWithCart()
    {
        // Arrange
        var userIdGuid = Guid.NewGuid();
        var query = new GetCartByUserIdQuery(userIdGuid);

        var expectedCart =  ProductCart.Create(UserId.From(userIdGuid));
        _cartRepository.GetCartByUserId(
                Arg.Is<UserId>(u => u.Value == userIdGuid),
                Arg.Any<CancellationToken>())
            .Returns(Result<ProductCart>.Success(expectedCart));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expectedCart);
        await _cartRepository.Received(1).GetCartByUserId(
            Arg.Is<UserId>(u => u.Value == userIdGuid),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CartNotFound_ReturnsCartWithUserIdNotFoundError()
    {
        // Arrange
        var userIdGuid = Guid.NewGuid();
        var query = new GetCartByUserIdQuery(userIdGuid);

        var notFoundError = new ICartRepository.CartWithUserIdNotFoundError(userIdGuid);
        _cartRepository.GetCartByUserId(
                Arg.Is<UserId>(u => u.Value == userIdGuid),
                Arg.Any<CancellationToken>())
            .Returns(Result<ProductCart>.Failure(notFoundError));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.CartWithUserIdNotFoundError);
        await _cartRepository.Received(1).GetCartByUserId(
            Arg.Is<UserId>(u => u.Value == userIdGuid),
            Arg.Any<CancellationToken>());
    }
}