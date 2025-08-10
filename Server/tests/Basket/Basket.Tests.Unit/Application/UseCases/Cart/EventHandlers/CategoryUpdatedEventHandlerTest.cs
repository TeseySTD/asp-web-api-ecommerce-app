using System.Linq.Expressions;
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
using Shared.Messaging.Events.Category;

namespace Basket.Tests.Unit.Application.UseCases.Cart.EventHandlers;

public class CategoryUpdatedEventHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly CategoryUpdatedEventHandler _handler;
    private readonly ILogger<CategoryUpdatedEventHandler> _logger;

    public CategoryUpdatedEventHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _logger = Substitute.For<ILogger<CategoryUpdatedEventHandler>>();
        _handler = new CategoryUpdatedEventHandler(_logger, _cartRepository);
    }


    [Fact]
    public async Task Handle_CartsFound_UpdatesItemsAndSavesEachCart()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var userId = UserId.From(Guid.NewGuid());
        var testCategory = ProductCartItemCategory.Create(
            CategoryId.Create(Guid.NewGuid()).Value,
            CategoryName.Create("Test category").Value
        );

        var cartWithItemWithCategory = ProductCart.Create(userId);
        var item = ProductCartItem.Create(
            ProductId.Create(productId).Value,
            ProductTitle.Create("Title").Value,
            StockQuantity.Create(3).Value,
            ProductPrice.Create(10m).Value,
            testCategory
        );
        cartWithItemWithCategory.AddItem(item);
        
        _cartRepository.GetCartsByPredicate(
                Arg.Any<Expression<Func<ProductCart, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(Result<IEnumerable<ProductCart>>.Success([cartWithItemWithCategory]));
        _cartRepository.SaveCart(cartWithItemWithCategory)
            .Returns(Result<ProductCart>.Success(cartWithItemWithCategory));

        var @event = new CategoryUpdatedEvent(testCategory.Id.Value, "New name");

        var context = Substitute.For<ConsumeContext<CategoryUpdatedEvent>>();
        context.Message.Returns(@event);

        // Act
        await _handler.Handle(context);

        // Assert
        var updatedItem = cartWithItemWithCategory.Items.First();
        updatedItem.Category.Should().NotBeNull();
        updatedItem.Category.Id.Value.Should().Be(@event.CategoryId);
        updatedItem.Category.CategoryName.Value.Should().Be(@event.CategoryName);

        await _cartRepository.Received(1).SaveCart(cartWithItemWithCategory);
    }
}