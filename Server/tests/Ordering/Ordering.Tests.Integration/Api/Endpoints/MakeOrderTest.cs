using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Ordering.API.Http.Order.Requests;
using Ordering.Application.UseCases.Orders.Commands.CreateOrder;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Api.Endpoints;

public class MakeOrderTest : ApiTest
{
    public const string RequestUrl = "/api/orders";

    public MakeOrderTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private MakeOrderRequest CreateTestRequest() => new(
        [new(Guid.NewGuid(), 10)],
        CardName: "John Doe",
        CardNumber: "4111111111111111",
        Expiration: "12/25",
        CVV: "123",
        PaymentMethod: "Visa",
        AddressLine: "456 Oak Rd",
        Country: "USA",
        State: "CA",
        ZipCode: "12345"
    );

    private HttpRequestMessage GenerateHttpRequest(MakeOrderRequest dto, Guid userId)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Default",
            ["userId"] = userId.ToString()
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");

        return request;
    }

    [Fact]
    public async Task WhenUnathorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var dto = CreateTestRequest();
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenOrderItemIsNotUnique_ThenShouldReturnBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var dto = CreateTestRequest() with
        {
            OrderItems =
            [
                new(productId, 10),
                new(productId, 8)
            ]
        };

        var request = GenerateHttpRequest(dto, Guid.NewGuid());

        var expectedContent = MakeSystemErrorApiOutput(new CreateOrderCommandHandler.OrderItemIsNotUniqueError());

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenDataIsCorrect_ThenShouldReturnOk()
    {
        var dto = CreateTestRequest();

        var request = GenerateHttpRequest(dto, Guid.NewGuid());

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}