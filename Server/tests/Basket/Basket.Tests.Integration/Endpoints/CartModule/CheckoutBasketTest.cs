using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Basket.API.Data.Abstractions;
using Basket.API.Http.Cart.Requests;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using Newtonsoft.Json;

namespace Basket.Tests.Integration.Endpoints.CartModule;

public class CheckoutBasketTest : ApiTest
{
    public CheckoutBasketTest(DatabaseFixture databaseFixture, IntegrationTestWebApplicationFactory factory) : base(
        databaseFixture, factory)
    {
    }

    private const string RequestUrl = "/api/cart/checkout";

    private CheckoutBasketRequest GenerateTestCheckoutBasketRequest(Guid userId) => new
    (
        UserId: userId,
        CardName: "John Doe",
        CardNumber: "4111111111111111",
        Expiration: "12/25",
        CVV: "123",
        PaymentMethod: "Visa",
        AddressLine: "123 Test Street",
        Country: "Ukraine",
        State: "Kyiv",
        ZipCode: "01001"
    );

    private HttpRequestMessage GetRequestWithAuth(Guid userId)
    {
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>()
        {
            ["role"] = "Default"
        });
        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(GenerateTestCheckoutBasketRequest(userId)),
            Encoding.UTF8,
            "application/json"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        return request;
    }

    [Fact]
    public async Task WhenUnathorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = GenerateTestCheckoutBasketRequest(userId);

        // Act
        var result = await HttpClient.PostAsJsonAsync(RequestUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task WhenCartNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = GetRequestWithAuth(userId);

        var expectedJson = MakeSystemErrorApiOutput(new ICartRepository.CartWithUserIdNotFoundError(userId));

        // Act
        var result = await HttpClient.SendAsync(request);
        var actualJson = await result.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }


    [Fact]
    public async Task WhenValidRequest_ThenReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = ProductCart.Create(UserId.From(userId));

        var productId = Guid.NewGuid();
        cart.AddItem(
            ProductCartItem.Create(
                ProductId.Create(productId).Value,
                ProductTitle.Create("Test Product").Value,
                StockQuantity.Create(10).Value,
                ProductPrice.Create(100).Value,
                ProductCartItemCategory.Create(
                    CategoryId.Create(Guid.NewGuid()).Value,
                    CategoryName.Create("Test Category").Value
                )
            )
        );

        Session.Store(cart);
        await Session.SaveChangesAsync();

        var request = GetRequestWithAuth(userId);

        // Act
        var result = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}