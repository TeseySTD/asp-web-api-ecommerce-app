using System.Net;
using System.Text;
using System.Text.Json;
using Shared.Core.Validation.Result;
using Users.API.Http.Auth.Requests;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.AuthenticationModule;

public class LoginTest : ApiTest
{
    public const string RequestUrl = "/api/auth/login";

    public LoginTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    [Fact]
    public async Task WhenUserIsNotRegistered_ThenBadRequestIsReturned()
    {
        // Arrange
        var userEmail = "test@test.com";
        var userPassword = "test test test";
        using StringContent content = new(
            JsonSerializer.Serialize(new LoginUserRequest(userEmail, userPassword)),
            Encoding.UTF8,
            "application/json"
        );
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        request.Content = content;

        var expectedJson = MakeSystemErrorApiOutput("Incorrect email", $"User with email {userEmail} does not exist");

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task WhenPasswordIsNotOfUser_ThenBadRequestIsReturned()
    {
        // Arrange
        var userName = "test";
        var userEmail = "test@test.com";
        var userPassword = "test test test";
        var userIncorrectPassword = "test test test2";
        var userPhoneNumber = "+380991444230";
        var userRole = "Default";

        using StringContent c = new(
            JsonSerializer.Serialize(new RegisterUserRequest(userName, userEmail, userPassword,
                userPhoneNumber, userRole)),
            Encoding.UTF8,
            "application/json"
        );

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register");
        req.Content = c;
        await HttpClient.SendAsync(req);

        using StringContent content = new(
            JsonSerializer.Serialize(new LoginUserRequest(userEmail, userIncorrectPassword)),
            Encoding.UTF8,
            "application/json"
        );
        var request = new HttpRequestMessage(HttpMethod.Post, RequestUrl);
        request.Content = content;

        var expectedJson = MakeSystemErrorApiOutput("Incorrect password",
            $"User with email {userEmail} and password '{userIncorrectPassword}' does not exist");

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task WhenCredentialsAreCorrecct_ThenOkIsReturned()
    {
        // Arrange
        var userName = "test";
        var userEmail = "test@test.com";
        var userPassword = "test test test";
        var userPhoneNumber = "+380991444230";
        var userRole = "Default";

        using StringContent c = new(
            JsonSerializer.Serialize(new RegisterUserRequest(userName, userEmail, userPassword,
                userPhoneNumber, userRole)),
            Encoding.UTF8,
            "application/json"
        );

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register");
        req.Content = c;
        await HttpClient.SendAsync(req);
        
        using StringContent content = new(
            JsonSerializer.Serialize(new LoginUserRequest(userEmail, userPassword)),
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