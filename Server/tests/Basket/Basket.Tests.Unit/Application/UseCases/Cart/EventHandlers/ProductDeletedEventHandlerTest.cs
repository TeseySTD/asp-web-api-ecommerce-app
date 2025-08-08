using Basket.API.Application.UseCases.Cart.EventHandlers;
using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Core.Validation.Result;
using Shared.Messaging.Events.Product;

namespace Basket.Tests.Unit.Application.UseCases.Cart.EventHandlers;

public class ProductDeletedEventHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly ProductDeletedEventHandler _handler;
    private readonly ILogger<ProductDeletedEventHandler> _logger;

    public ProductDeletedEventHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _logger = Substitute.For<ILogger<ProductDeletedEventHandler>>();
        _handler = new ProductDeletedEventHandler(_logger, _cartRepository);
    }

    private ProductCartItem CreateTestProductCartItem(ProductId? productId = null) => ProductCartItem.Create(
        productId ?? ProductId.From(Guid.NewGuid()),
        ProductTitle.Create("Title").Value,
        StockQuantity.Create(3).Value,
        ProductPrice.Create(10m).Value,
        null
    );

    [Fact]
    public async Task WhenNoCartsFound_ThenLogsInfoAndDoesNotSave()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var @event = new ProductDeletedEvent(productId);

        _cartRepository.GetCartsByProductId(
                Arg.Is<ProductId>(p => p.Value == productId),
                Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProductCart>>.Failure(
                new ICartRepository.CartWithProductIdNotFoundError(productId)));

        var context = Substitute.For<ConsumeContext<ProductDeletedEvent>>();
        context.Message.Returns(@event); 
        
        // Act
        await _handler.Handle(context);

        // Assert
        await _cartRepository.DidNotReceive().SaveCart(Arg.Any<ProductCart>());
        _logger.Received(1).LogInformation("Carts not found");
    }

    [Fact]
    public async Task WhenCartsFound_ThenDeleteItemsAndSavesEachCart()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var productId = ProductId.From(Guid.NewGuid());

        var cartWithProduct = ProductCart.Create(userId);
        cartWithProduct.AddItem(CreateTestProductCartItem(productId));
        cartWithProduct.AddItem(CreateTestProductCartItem(productId));
        cartWithProduct.AddItem(CreateTestProductCartItem());

        _cartRepository.GetCartsByProductId(
                Arg.Is<ProductId>(pid => pid == productId),
                Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProductCart>>.Success([cartWithProduct]));
        _cartRepository.SaveCart(cartWithProduct)
            .Returns(Result<ProductCart>.Success(cartWithProduct));


        var @event = new ProductDeletedEvent(productId.Value);

        var context = Substitute.For<ConsumeContext<ProductDeletedEvent>>();
        context.Message.Returns(@event);

        // Act
        await _handler.Handle(context);

        // Assert
        cartWithProduct.Items.Should().HaveCount(1);
        cartWithProduct.Items.Should().ContainSingle(p => p.Id != productId);
    }
}