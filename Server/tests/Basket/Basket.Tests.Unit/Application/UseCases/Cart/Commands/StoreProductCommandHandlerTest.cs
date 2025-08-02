using Basket.API.Application;
using Basket.API.Application.UseCases.Cart.Commands.StoreProduct;
using Basket.API.Data.Abstractions;
using Basket.API.Dto.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;
using NSubstitute;
using Shared.Core.Validation.Result;

namespace Basket.Tests.Unit.Application.UseCases.Cart.Commands;

public class StoreProductCommandHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly StoreProductCommandHandler _handler;

    public StoreProductCommandHandlerTest()
    {
        MapsterConfig.Configure(Substitute.For<IServiceProvider>());

        _cartRepository = Substitute.For<ICartRepository>();
        _handler = new StoreProductCommandHandler(_cartRepository);
    }

    private ProductCartItemDto CreateValidProductDto(Guid productId)
    {
        var categoryDto = new ProductCartItemCategoryDto(
            Guid.NewGuid(),
            "CategoryName"
        );

        return new ProductCartItemDto(
            productId,
            "Test Product",
            2u,
            19.99m,
            Array.Empty<string>(),
            categoryDto
        );
    }

    [Fact]
    public async Task WhenProductAlreadyInCart_ThenReturnsProductAlreadyInCartError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productDto = CreateValidProductDto(productId);
        var command = new StoreProductCommand(userId, productDto);

        var error = new ICartRepository.ProductAlreadyInCartError(productId);
        _cartRepository.StoreProductInCart(
                Arg.Is<UserId>(u => u.Value == userId),
                Arg.Is<ProductCartItem>(item => item.Id.Value == productId),
                Arg.Any<CancellationToken>())
            .Returns(error);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ICartRepository.ProductAlreadyInCartError);
        await _cartRepository.Received(1).StoreProductInCart(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Is<ProductCartItem>(item => item.Id.Value == productId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WhenValidRequest_ThenReturnsSuccessResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productDto = CreateValidProductDto(productId);
        var command = new StoreProductCommand(userId, productDto);

        _cartRepository.StoreProductInCart(
                Arg.Any<UserId>(),
                Arg.Any<ProductCartItem>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _cartRepository.Received(1).StoreProductInCart(
            Arg.Is<UserId>(u => u.Value == userId),
            Arg.Is<ProductCartItem>(item => item.Id.Value == productId && item.StockQuantity.Value == 2),
            Arg.Any<CancellationToken>());
    }
}