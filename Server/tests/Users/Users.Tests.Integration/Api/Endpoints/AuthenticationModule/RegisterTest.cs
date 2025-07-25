using System.Net;
using System.Text;
using System.Text.Json;
using Users.API.Http.Auth.Requests;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.AuthenticationModule;

public class RegisterTest : ApiTest
{
    public const string RequestUrl = "/api/auth/register";

    public RegisterTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(
        factory, databaseFixture)
    {
    }

    [Fact]
    public async Task WhenUserDataIncorrect_ThenBadRequestIsReturned()
    {
        // Arrange 
        var incorrectUserName = "";
        var incorrectPassword = "123";
        var incorrectEmail = "incorrect.email";
        var incorrectPhoneNumber = "0123456789";
        var incorrectRole = "incorrect.role";

        using StringContent content = new(
            JsonSerializer.Serialize(new RegisterUserRequest(incorrectUserName, incorrectEmail, incorrectPassword,
                incorrectPhoneNumber, incorrectRole)),
            Encoding.UTF8,
            "application/json"
        );
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        request.Content = content;

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert 
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WhenUserDataCorrect_ThenOkIsReturned()
    {
        // Arrange 
        var userName = "test";
        var userEmail = "test@test.com";
        var userPassword = "test test test";
        var userPhoneNumber = "+380991444230";
        var userRole = "Default";

        using StringContent content = new(
            JsonSerializer.Serialize(new RegisterUserRequest(userName, userEmail, userPassword,
                userPhoneNumber, userRole)),
            Encoding.UTF8,
            "application/json"
        );
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        request.Content = content;

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}