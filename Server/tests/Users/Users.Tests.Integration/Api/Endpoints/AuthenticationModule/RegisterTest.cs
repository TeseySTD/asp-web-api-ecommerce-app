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
    public async Task Register_UserDataIncorrect_BadRequestIsReturned()
    {
        // Arrange 
        var incorrectUserName = "";
        var incorrectPassword = "123";
        var incorrectEmail = "incorrect.email";
        var incorrectPhoneNumber = "0123456789";
        var incorrectRole = "incorrect.role";

        var json = JsonSerializer.Serialize(new RegisterUserRequest(incorrectUserName, incorrectEmail, incorrectPassword, incorrectPhoneNumber, incorrectRole));
        using StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);

        // Assert 
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_UserDataIsCorrect_OkIsReturned()
    {
        // Arrange 
        var userName = "test";
        var userEmail = "test@test.com";
        var userPassword = "test test test";
        var userPhoneNumber = "+380991444230";
        var userRole = "Default";

        var json = JsonSerializer.Serialize(new RegisterUserRequest(userName, userEmail, userPassword, userPhoneNumber, userRole));
        using StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}