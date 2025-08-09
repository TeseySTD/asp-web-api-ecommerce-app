using System.Net;
using System.Net.Http.Headers;
using Basket.API.Data.Abstractions;
using Basket.API.Models.Cart;
using Basket.API.Models.Cart.ValueObjects;
using Basket.Tests.Integration.Common;

namespace Basket.Tests.Integration.Endpoints.CartModule;

public class DeleteCartTest : ApiTest
{
    public DeleteCartTest(DatabaseFixture databaseFixture, IntegrationTestWebApplicationFactory factory) : base(
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
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{userId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        return request;
    }

    [Fact]
    public async Task DeleteCart_UserUnauthorized_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCart_CartIsNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var request = GetRequestWithAuth(userId);

        var expectedJson = MakeSystemErrorApiOutput(new ICartRepository.CartWithUserIdNotFoundError(userId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task DeleteCart_CartIsFound_ReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = ProductCart.Create(UserId.From(userId));

        Session.Store(cart);
        await Session.SaveChangesAsync();

        var request = GetRequestWithAuth(userId); 
        
        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}