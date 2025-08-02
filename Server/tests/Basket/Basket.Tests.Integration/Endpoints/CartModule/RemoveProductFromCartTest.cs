using System.Net;
using System.Net.Http.Headers;
using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;

namespace Basket.Tests.Integration.Endpoints.CartModule;

public class RemoveProductFromCartTest : ApiTest
{
    public RemoveProductFromCartTest(DatabaseFixture databaseFixture, IntegrationTestWebApplicationFactory factory) :
        base(databaseFixture, factory)
    {
    }

    private const string RequestUrl = "/api/cart";

    private HttpRequestMessage GetRequestWithAuth(Guid userId, Guid productId)
    {
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>()
        {
            ["role"] = "Default"
        });

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{userId}/{productId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task WhenUnathorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userId}/{productId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCartNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var expectedJson = MakeSystemErrorApiOutput(new ICartRepository.CartWithUserIdNotFoundError(userId));
        
        var request = GetRequestWithAuth(userId, productId);

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenItemNotExist_ThenReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = ProductCart.Create(UserId.From(userId));

        Session.Store(cart);
        await Session.SaveChangesAsync();

        var expectedJson = MakeSystemErrorApiOutput(new ICartRepository.ProductInCartNotFound(productId));

        var request = GetRequestWithAuth(userId, productId);

        // Act
        var response = await HttpClient.SendAsync(request);

        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenItemExists_ThenRemovesItemAndReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var cart = ProductCart.Create(UserId.From(userId));
        var item = ProductCartItem.Create(
            ProductId.From(productId),
            ProductTitle.Create("T").Value,
            StockQuantity.Create(2).Value,
            ProductPrice.Create(10m).Value,
            ProductCartItemCategory.Create(CategoryId.Create(Guid.NewGuid()).Value, CategoryName.Create("C").Value)
        );

        cart.AddItem(item);
        Session.Store(cart);
        await Session.SaveChangesAsync();

        var request = GetRequestWithAuth(userId, productId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}