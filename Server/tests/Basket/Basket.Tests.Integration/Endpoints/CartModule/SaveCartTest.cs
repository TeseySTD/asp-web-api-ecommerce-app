using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Basket.API.Dto.Cart;
using Basket.API.Http.Cart.Requests;
using Basket.Tests.Integration.Common;
using Newtonsoft.Json;

namespace Basket.Tests.Integration.Endpoints.CartModule;

public class SaveCartTest : ApiTest
{
    public SaveCartTest(DatabaseFixture databaseFixture, IntegrationTestWebApplicationFactory factory) : base(
        databaseFixture, factory)
    {
    }

    private const string RequestUrl = "/api/cart";

    private HttpRequestMessage GetRequestWithAuth(ProductCartDto dto)
    {
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>()
        {
            ["role"] = "Default"
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}/");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(new SaveCartRequest(dto)),
            Encoding.UTF8,
            "application/json"
        );
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private ProductCartDto CreateTestProductCartDto(Guid userId) => new ProductCartDto(
        UserId: userId,
        Items:
        [
            new ProductCartItemDto(
                Guid.NewGuid(),
                "Test Product",
                2u,
                9.99m,
                Array.Empty<string>(),
                new ProductCartItemCategoryDto(Guid.NewGuid(), "Category1"))
        ]
    );

    [Fact]
    public async Task WhenUnathorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = CreateTestProductCartDto(userId);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}/");
        request.Content = new StringContent(
            JsonConvert.SerializeObject(new SaveCartRequest(dto)),
            Encoding.UTF8,
            "application/json"
        );

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenSaveSucceeds_ThenReturnsOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = CreateTestProductCartDto(userId);
        var request = GetRequestWithAuth(dto);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}