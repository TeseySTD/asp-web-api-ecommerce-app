using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Basket.API.Data.Abstractions;
using Basket.API.Dto.Cart;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;
using FluentAssertions;
using Mapster;

namespace Basket.Tests.Integration.Endpoints.CartModule;

public class GetCartTest : ApiTest
{
    public GetCartTest(DatabaseFixture databaseFixture, IntegrationTestWebApplicationFactory factory) : base(
        databaseFixture, factory)
    {
    }

    private const string RequestUrl = "/api/cart";

    private HttpRequestMessage GetRequestWithAuth(Guid userId)
    {
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>()
        {
            ["role"] = "Default"
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestUrl}/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    [Fact]
    public async Task WhenUnathorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{userId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
    public async Task WhenCartIsInDb_ThenReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = ProductCart.Create(UserId.From(userId));

        Session.Store(cart);
        await Session.SaveChangesAsync();

        var request = GetRequestWithAuth(userId);
        
        // Act
        var result = await HttpClient.SendAsync(request);
        var resultCartDto = await result.Content.ReadFromJsonAsync<ProductCartDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(resultCartDto);
        var cartDto = cart.Adapt<ProductCartDto>();
        resultCartDto.Should().BeEquivalentTo(cartDto);
    }
}