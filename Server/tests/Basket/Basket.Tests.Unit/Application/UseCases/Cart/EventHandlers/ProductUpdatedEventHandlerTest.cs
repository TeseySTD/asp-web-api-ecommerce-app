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

public class ProductUpdatedEventHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly ProductUpdatedEventHandler _handler;
    private readonly ILogger<ProductUpdatedEventHandler> _logger;

    public ProductUpdatedEventHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _logger = Substitute.For<ILogger<ProductUpdatedEventHandler>>();
        _handler = new ProductUpdatedEventHandler(_logger, _cartRepository);
    }

    private static ProductUpdatedEvent CreateEvent(Guid productId)
    {
        return new ProductUpdatedEvent(
            ProductId: productId,
            Title: "NewTitle",
            Description: "Desc",
            Price: 42m,
            Category: new ProductUpdatedEventCategory(productId, "CatName"),
            ImageUrls: ["url1", "url2"]
        );
    }

    [Fact]
    public async Task WhenNoCartsFound_LogsInfoAndDoesNotSave()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var @event = CreateEvent(productId);

        _cartRepository.GetCartsByProductId(
                Arg.Is<ProductId>(p => p.Value == productId),
                Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProductCart>>.Failure(
                new ICartRepository.CartWithProductIdNotFoundError(productId)));

        var context = Substitute.For<ConsumeContext<ProductUpdatedEvent>>();
        context.Message.Returns(@event);

        // Act
        await _handler.Handle(context);

        // Assert
        await _cartRepository.DidNotReceive().SaveCart(Arg.Any<ProductCart>());
        _logger.Received(1).LogInformation("Carts not found");
    }

    [Fact]
    public async Task WhenCartsFound_UpdatesItemsAndSavesEachCart()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var @event = CreateEvent(productId);
        var userId = UserId.From(Guid.NewGuid());

        var cartWithItem = ProductCart.Create(userId);
        var item = ProductCartItem.Create(
            ProductId.Create(productId).Value,
            ProductTitle.Create("OldTitle").Value,
            StockQuantity.Create(3).Value,
            ProductPrice.Create(10m).Value,
            ProductCartItemCategory.Create(
                CategoryId.Create(Guid.NewGuid()).Value,
                CategoryName.Create("OldCat").Value)
        );
        cartWithItem.AddItem(item);

        var otherCart = ProductCart.Create(userId);

        var carts = new List<ProductCart> { cartWithItem, otherCart };
        _cartRepository.GetCartsByProductId(
                Arg.Is<ProductId>(p => p.Value == productId),
                Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProductCart>>.Success(carts));

        var context = Substitute.For<ConsumeContext<ProductUpdatedEvent>>();
        context.Message.Returns(@event);

        // Act
        await _handler.Handle(context);

        // Assert
        var updatedItem = cartWithItem.Items.Single(i => i.Id.Value == productId);
        updatedItem.Title.Value.Should().Be("NewTitle");
        updatedItem.Price.Value.Should().Be(42m);
        updatedItem.ImageUrls.Should().Equal(["url1", "url2"]);
        updatedItem.Category.Should().NotBeNull();
        updatedItem.Category!.CategoryName.Value.Should().Be("CatName");

        // SaveCart called once for each cart (including the one without matching items)
        await _cartRepository.Received(1).SaveCart(cartWithItem);
        await _cartRepository.Received(1).SaveCart(otherCart);
    }
}