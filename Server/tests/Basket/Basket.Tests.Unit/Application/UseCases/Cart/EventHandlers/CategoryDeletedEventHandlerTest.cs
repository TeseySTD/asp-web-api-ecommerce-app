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

public class CategoryDeletedEventHandlerTest
{
    private readonly ICartRepository _cartRepository;
    private readonly CategoryDeletedEventHandler _handler;
    private readonly ILogger<CategoryDeletedEventHandler> _logger;

    public CategoryDeletedEventHandlerTest()
    {
        _cartRepository = Substitute.For<ICartRepository>();
        _logger = Substitute.For<ILogger<CategoryDeletedEventHandler>>();
        _handler = new(_logger, _cartRepository);
    }

    [Fact]
    public async Task WhenCartsFound_ThenUpdatesItemsDeletesCategoryAndSavesEachCart()
    {
        // Arrange
        var userId = UserId.From(Guid.NewGuid());
        var categoryId = CategoryId.From(Guid.NewGuid());
        var testCategory = ProductCartItemCategory.Create(
            categoryId: categoryId,
            CategoryName.Create("Test category").Value
        );

        var cartWithItemWithCategory = ProductCart.Create(userId);
        var item = ProductCartItem.Create(
            ProductId.Create(Guid.NewGuid()).Value,
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

        var @event = new CategoryDeletedEvent(categoryId.Value);

        var context = Substitute.For<ConsumeContext<CategoryDeletedEvent>>();
        context.Message.Returns(@event);

        // Act
        await _handler.Handle(context);

        // Assert
        var updatedItem = cartWithItemWithCategory.Items.First();
        updatedItem.Category.Should().BeNull();
        
        await _cartRepository.Received(1).SaveCart(cartWithItemWithCategory);
    }
}