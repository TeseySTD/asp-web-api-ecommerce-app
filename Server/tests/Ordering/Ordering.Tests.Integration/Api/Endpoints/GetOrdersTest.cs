using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using Ordering.Application.UseCases.Orders.Queries.GetOrders;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Api.Endpoints;

public class GetOrdersTest : ApiTest
{
    public const string RequestUrl = "/api/orders";

    public GetOrdersTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private List<Order> GetTestListOrders(Guid customerId) =>
    [
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("Jane Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.InProgress
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("Somebody One", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.InProgress
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.InProgress
        ),
        Order.Create(
            CustomerId.Create(customerId).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value,
            OrderStatus.Completed
        )

    ];

    private HttpRequestMessage GenerateHttpRequest(string url, Guid? userId = null, string role = "Default")
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
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
        // Act
        var response = await HttpClient.GetAsync(RequestUrl + "/");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenCustomerRequestsOtherCustomersData_ThenReturnsForbidden()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = GenerateHttpRequest(RequestUrl + $"/?customerId={customerId}", Guid.NewGuid());

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task WhenNoOrdersExist_ThenReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var request = GenerateHttpRequest(RequestUrl + $"/?customerId={customerId}", customerId);

        var expectedContent = MakeSystemErrorApiOutput(new GetOrdersQueryHandler.OrdersNotFoundError());

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenRequestIsOutOfRange_ThenReturnsNotFound()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = GetTestListOrders(customerId);

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(RequestUrl + $"/?customerId={customerId}&pageIndex=1&pageSize={orders.Count}",
            customerId);
        var expectedContent = MakeSystemErrorApiOutput(new GetOrdersQueryHandler.OrdersNotFoundError());

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenRequestIsValid_ThenReturnsOk()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = GetTestListOrders(customerId);
        var filteredOrders = orders.Where(o => o.Status == OrderStatus.InProgress);

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(RequestUrl + $"/?customerId={customerId}&pageIndex=0&orderStatus=inprogress", customerId);

        // Act
        var response = await HttpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        foreach (var o in filteredOrders)
        {
            json.Should()
                .Contain(o.CustomerId.Value.ToString()).And
                .Contain(o.Status.ToString()).And
                .Contain(o.Id.Value.ToString());
        }
    }

    [Fact]
    public async Task WhenRequestIsValidAndCustomerIsAdmin_ThenReturnsOk()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orders = GetTestListOrders(customerId);

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(RequestUrl + $"/?customerId={customerId}&pageIndex=0", Guid.NewGuid(), "Admin");

        // Act
        var response = await HttpClient.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        foreach (var o in orders)
        {
            json.Should()
                .Contain(o.CustomerId.Value.ToString());
        }
    }
}