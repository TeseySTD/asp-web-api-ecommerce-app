using System.Net;
using System.Net.Http.Headers;
using Shared.Core.Auth;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.AuthenticationModule;

public class LogoutTest : ApiTest
{
    public const string RequestUrl = "/api/auth/logout";

    public LogoutTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private User CreateTestUser() => User.Create(
        name: UserName.Create("test").Value,
        email: Email.Create("test@gmail.com").Value,
        hashedPassword: HashedPassword.Create("12345678").Value,
        phoneNumber: PhoneNumber.Create("+380991444743").Value,
        role: UserRole.Default
    );

    [Fact]
    public async Task WhenNotAuthenticated_ThenReturnsUnauthorized()
    {
        // Arrange 
        var request = new HttpRequestMessage(HttpMethod.Delete, RequestUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task WhenLogoutForNonExistingUser_ThenReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser();

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenProvider.GenerateJwtToken(user));

        var request = new HttpRequestMessage(HttpMethod.Delete, RequestUrl);

        var expectedContent =
            MakeSystemErrorApiOutput("User does not exists", $"User with id {user.Id.Value} does not exists.");

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
        var user = CreateTestUser();
        
        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync();
        
        HttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenProvider.GenerateJwtToken(user));
        
        var request = new HttpRequestMessage(HttpMethod.Delete, RequestUrl);
        
        // Act
        var response = await HttpClient.SendAsync(request);
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}