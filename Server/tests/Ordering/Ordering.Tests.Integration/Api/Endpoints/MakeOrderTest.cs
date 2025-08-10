using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Ordering.API.Http.Order.Requests;
using Ordering.Application.UseCases.Orders.Commands.CreateOrder;
using Ordering.Core.Models.Products;
using Ordering.Core.Models.Products.ValueObjects;
using Ordering.Tests.Integration.Common;

namespace Ordering.Tests.Integration.Api.Endpoints;

public class MakeOrderTest : ApiTest
{
    public const string RequestUrl = "/api/orders";

    public MakeOrderTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private MakeOrderRequest CreateTestRequest(CreateOrderItemRequest[] items) => new(
        items,
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
    public async Task MakeOrder_Unathorized_ReturnsUnauthorized()
    {
        // Arrange
        var dto = CreateTestRequest([]);
        var json = JsonConvert.SerializeObject(dto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MakeOrder_OrderItemIsNotUnique_ReturnsBadRequest()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var dto = CreateTestRequest(
            [
                new(productId, 10),
                new(productId, 8)
            ]
        );

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
    public async Task MakeOrder_ProductsNotInDb_ReturnsBadRequest()
    {
        // Arrange
        Guid[] nonExistingProductIds = [Guid.NewGuid(), Guid.NewGuid()];
        var dto = CreateTestRequest(
            [
                new(nonExistingProductIds[0], 10),
                new(nonExistingProductIds[1], 8)
            ]
        );
        var request = GenerateHttpRequest(dto, Guid.NewGuid());

        var expectedContent = MakeSystemErrorApiOutput(new CreateOrderCommandHandler.ProductsNotFound(nonExistingProductIds));
        
        // Act
        var response = await HttpClient.SendAsync(request);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task MakeOrder_DataIsCorrect_ReturnsOk()
    {
        var product1 = Product.Create(
            id: ProductId.Create(Guid.NewGuid()).Value,
            title: ProductTitle.Create("Test #1").Value,
            description: ProductDescription.Create("Test Description #1").Value
        );
        var product2 = Product.Create(
            id: ProductId.Create(Guid.NewGuid()).Value,
            title: ProductTitle.Create("Test #2").Value,
            description: ProductDescription.Create("Test Description #2").Value
        );

        ApplicationDbContext.Products.AddRange(product1, product2);
        await ApplicationDbContext.SaveChangesAsync();

        var dto = CreateTestRequest(
            [
                new(product1.Id.Value, 10),
                new(product2.Id.Value, 8)
            ]
        );
        var request = GenerateHttpRequest(dto, Guid.NewGuid());

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}