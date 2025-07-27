using System.Net;
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

    [Fact]
    public async Task WhenOrderIsNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestUrl}/{orderId}");

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
        var order = Order.Create(
            CustomerId.Create(Guid.NewGuid()).Value,
            Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
            Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
            [],
            OrderId.Create(Guid.NewGuid()).Value
        );

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestUrl}/{order.Id.Value}");
        
        // Act
        var response = await HttpClient.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}