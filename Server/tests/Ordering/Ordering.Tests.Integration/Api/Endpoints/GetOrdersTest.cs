using System.Net;
using FluentAssertions;
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

    private List<Order> GetTestListOrders() =>
    [
        Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value
        ),
        Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("Jane Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            OrderId.Create(Guid.NewGuid()).Value
        ),
        Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("Somebody One", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            OrderId.Create(Guid.NewGuid()).Value
        )
    ];

    [Fact]
    public async Task WhenNoOrdersExist_ThenReturnsNotFound()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, RequestUrl + "/");

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
        var orders = GetTestListOrders();

        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, RequestUrl + $"/?pageIndex=1&pageSize={GetTestListOrders().Count}");
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
        var orders = GetTestListOrders();
        
        ApplicationDbContext.Orders.AddRange(orders);
        await ApplicationDbContext.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, RequestUrl + $"/?pageIndex=0");
        
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