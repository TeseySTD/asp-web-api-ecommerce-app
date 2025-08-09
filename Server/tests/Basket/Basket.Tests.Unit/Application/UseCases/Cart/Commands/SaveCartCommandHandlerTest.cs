using Basket.API.Application;
using Basket.API.Application.UseCases.Cart.Commands.SaveCart;
using Basket.API.Data.Abstractions;
using Basket.API.Dto.Cart;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Basket.Tests.Unit.Application.UseCases.Cart.Commands;

public class SaveCartCommandHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly SaveCartCommandHandler _handler;

    public SaveCartCommandHandlerTest()
    {
        MapsterConfig.Configure(Substitute.For<IServiceProvider>()); 

        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new SaveCartCommandHandler(_cartRepository);
    }

    private  ProductCartDto CreateCartDto(Guid userId)
    {
        return new ProductCartDto(
            UserId: userId,
            Items: new[]
            {
                new ProductCartItemDto(
                    Guid.NewGuid(),
                    "Test Product",
                    2u,
                    9.99m,
                    Array.Empty<string>(),
                    new ProductCartItemCategoryDto(Guid.NewGuid(), "Category1"))
            });
    }

    [Fact]
    public async Task Handle_SaveSucceeds_ReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = CreateCartDto(userId);
        var command = new SaveCartCommand(dto);

        // Capture adapted cart
        ProductCart passedCart = null!;
        _cartRepository.SaveCart(
                Arg.Do<ProductCart>(c => passedCart = c),
                Arg.Any<CancellationToken>())
            .Returns(Result<ProductCart>.Success(
                    ProductCart.Create(UserId.From(userId))
                )
            );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Verify repository called with correctly adapted cart
        passedCart.Should().NotBeNull();
        passedCart.Id.Value.Should().Be(userId);
        passedCart.Items.Should().HaveCount(1);
        await _cartRepository.Received(1).SaveCart(Arg.Any<ProductCart>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_SaveFails_ReturnsFailureResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = CreateCartDto(userId);
        var command = new SaveCartCommand(dto);

        var saveError = new Error("Store error", "Failure reason");
        _cartRepository.SaveCart(
                Arg.Any<ProductCart>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<ProductCart>.Failure(saveError));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e == saveError);
        await _cartRepository.Received(1).SaveCart(Arg.Any<ProductCart>(), Arg.Any<CancellationToken>());
    }
}