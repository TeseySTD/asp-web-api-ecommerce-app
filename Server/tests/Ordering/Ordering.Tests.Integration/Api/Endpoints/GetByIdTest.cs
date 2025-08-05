using System.Net;
using System.Net.Http.Headers;
using Ordering.Application.UseCases.Orders.Queries.GetOrderById;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Api.Endpoints;

public class GetByIdTest : ApiTest
{
    public const string RequestUrl = "/api/orders";

    public GetByIdTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private Order CreateTestOrder() => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        [],
        OrderId.Create(Guid.NewGuid()).Value
    );


    private HttpRequestMessage GenerateHttpRequest(Guid orderId, Guid? userId = null, string role = "Default")
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestUrl}/{orderId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = role,
            ["userId"] = userId?.ToString() ?? Guid.NewGuid().ToString(),
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    [Fact]
    public async Task WhenUnauthorized_ThenReturnsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerIsNotOrderOwner_ThenReturnsForbidden()
    {
        // Arrange
        var order = CreateTestOrder();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(order.Id.Value);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WhenOrderIsNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = GenerateHttpRequest(orderId);

        var expectedContent = MakeSystemErrorApiOutput(new GetOrderByIdQueryHandler.OrderNotFoundError(orderId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenOrderIsFound_ThenReturnsOk()
    {
        // Arrange
        var order = CreateTestOrder();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(order.Id.Value, order.CustomerId.Value);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WhenOrderIsFoundCustomerIsAdminAndNotOrderOwner_ThenReturnsOk()
    {
        // Arrange
        var order = CreateTestOrder();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(order.Id.Value, role:"Admin");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}