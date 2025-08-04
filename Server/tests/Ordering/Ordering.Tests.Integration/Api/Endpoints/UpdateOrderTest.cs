using System.Net;
using System.Net.Http.Headers;
using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Ordering.API.Http.Order.Requests;
using Ordering.Application.UseCases.Orders.Commands.UpdateOrder;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Api.Endpoints;

public class UpdateOrderTest : ApiTest
{
    public const string RequestUrl = "/api/orders";

    public UpdateOrderTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(
        factory, databaseFixture)
    {
    }

    private Order CreateTestOrder(OrderStatus status = OrderStatus.NotStarted) => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        [],
        OrderId.Create(Guid.NewGuid()).Value,
        status
    );

    private UpdateOrderRequest CreateTestRequest() => new(
        CardName: "Jane Doe",
        CardNumber: "4111111111111111",
        Expiration: "09/26",
        CVV: "321",
        PaymentMethod: "Visa",
        AddressLine: "457 Oak Rd",
        Country: "UA",
        State: "",
        ZipCode: "12345"
    );

    private HttpRequestMessage GenerateRequest(Guid orderId, UpdateOrderRequest dto, Guid? userId = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, $"{RequestUrl}/{orderId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = "Default",
            ["userId"] = userId?.ToString() ?? Guid.NewGuid().ToString(),
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8,
            "application/json");


        return request;
    }

    [Fact]
    public async Task WhenOrderIsNotInDb_ThenReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        var request = GenerateRequest(orderId, CreateTestRequest());

        var expectedContent = MakeSystemErrorApiOutput(new UpdateOrderCommandHandler.OrderNotFoundError(orderId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenCustomerIsNotOrderOwner_ThenReturnsBadRequest()
    {
        // Arrange
        var order = CreateTestOrder();
        var customerId = Guid.NewGuid();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(order.Id.Value, CreateTestRequest(), customerId);

        var expectedContent = MakeSystemErrorApiOutput(new UpdateOrderCommandHandler.CustomerMismatchError(customerId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Theory]
    [InlineData(OrderStatus.Cancelled)]
    [InlineData(OrderStatus.Completed)]
    public async Task WhenOrderStateIsIncorrect_ThenReturnsBadRequest(OrderStatus orderStatus)
    {
        // Arrange
        var order = CreateTestOrder(orderStatus);
        var customerId = order.CustomerId.Value;

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(order.Id.Value, CreateTestRequest(), customerId);

        var expectedContent = MakeSystemErrorApiOutput(new UpdateOrderCommandHandler.IncorrectOrderStateError());

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenDataIsCorrect_ThenReturnsOk()
    {
        // Arrange
        var order = CreateTestOrder();
        var customerId = order.CustomerId.Value;

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateRequest(order.Id.Value, CreateTestRequest(), customerId);
        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}