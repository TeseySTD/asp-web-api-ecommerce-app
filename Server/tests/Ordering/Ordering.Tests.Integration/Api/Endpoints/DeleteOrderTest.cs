using System.Net;
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

    [Fact]
    public async Task WhenOrderIsNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{orderId}");

        var expectedContent = MakeSystemErrorApiOutput(new DeleteOrderCommandHandler.OrderNotFoundError(orderId));

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        //Assert
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
        
        var request = new HttpRequestMessage(HttpMethod.Delete, $"{RequestUrl}/{order.Id.Value}");
        
        // Act
        var response = await HttpClient.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var isOrderInDb = await ApplicationDbContext.Orders.AnyAsync(o => o.Id == order.Id);
        Assert.False(isOrderInDb);
    }
}