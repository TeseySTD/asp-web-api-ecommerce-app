using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.UseCases.Orders.Commands.DeleteOrder;
using Ordering.Core.Models.Orders;
using Ordering.Core.Models.Orders.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Api.Endpoints;

public class DeleteOrderTest : ApiTest
{
    public const string RequestUrl = "/api/orders";

    public DeleteOrderTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(
        factory, databaseFixture)
    {
    }

    private Order CreateTestOrder() => Order.Create(
        CustomerId.Create(Guid.NewGuid()).Value,
        Payment.Create("John Doe", "4111111111111111", "12/25", "123", "Visa").Value,
        Address.Create("456 Oak Rd", "USA", "CA", "12345").Value,
        OrderId.Create(Guid.NewGuid()).Value
    );

    private HttpRequestMessage GenerateHttpRequest(Guid orderId, Guid userId, string role = "Default")
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{orderId}");
        var token = TestJwtTokens.GenerateToken(new Dictionary<string, object>
        {
            ["role"] = role,
            ["userId"] = userId.ToString()
        });
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return request;
    }

    [Fact]
    public async Task DeleteOrder_Unathorized_ReturnsUnauthorized()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{orderId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteOrder_CustomerIsNotOrderOwner_ReturnsForbidden()
    {
        // Arrange
        var order = CreateTestOrder();
        var customerId = Guid.NewGuid();

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(order.Id.Value, customerId);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_OrderIsNotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = GenerateHttpRequest(orderId, Guid.NewGuid());

        var expectedContent = MakeSystemErrorApiOutput(new DeleteOrderCommandHandler.OrderNotFoundError(orderId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        //Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task DeleteOrder_OrderIsFound_ReturnsOk()
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
    public async Task DeleteOrder_OrderInDbCustomerIsAdminAndNotOrderOwner_ReturnsOk()
    {
        // Arrange
        var order = CreateTestOrder();
        var customerId = CustomerId.Create(Guid.NewGuid()).Value;

        ApplicationDbContext.Orders.Add(order);
        await ApplicationDbContext.SaveChangesAsync();

        var request = GenerateHttpRequest(order.Id.Value, customerId.Value, role: "Admin");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}