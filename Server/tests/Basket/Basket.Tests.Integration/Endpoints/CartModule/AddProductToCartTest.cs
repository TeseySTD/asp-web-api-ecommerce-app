using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Basket.API.Data.Abstractions;
using Basket.API.Dto.Cart;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.Entities;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using Newtonsoft.Json;

namespace Basket.Tests.Integration.Endpoints.CartModule;

public class AddProductToCartTest : ApiTest
{
    public AddProductToCartTest(DatabaseFixture databaseFixture, IntegrationTestWebApplicationFactory factory) : base(
        databaseFixture, factory)
    {
    }

    private const string RequestUrl = "/api/cart";

    private ProductCartItemDto CreateValidProductDto(Guid productId)
    {
        var categoryDto = new ProductCartItemCategoryDto(
            Guid.NewGuid(),
            "Test Category"
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

    private HttpRequestMessage GetRequestWithAuth(Guid userId, ProductCartItemDto cartItemDto)
    {
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>()
        {
            ["role"] = "Default"
        });
        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}/{userId}");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(cartItemDto),
            Encoding.UTF8,
            "application/json"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productDto = CreateValidProductDto(productId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}/{userId}");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(productDto),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenProductIsAlreadyInCart_ThenReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productDto = CreateValidProductDto(productId);
        var cart = ProductCart.Create(UserId.From(userId));
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

        var request = GetRequestWithAuth(userId, productDto);

        var expectedJson = MakeSystemErrorApiOutput(new ICartRepository.ProductAlreadyInCartError(productId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenRequestIsCorrect_ThenReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var productDto = CreateValidProductDto(productId);

        var request = GetRequestWithAuth(userId, productDto);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}