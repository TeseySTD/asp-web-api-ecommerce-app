using System.Net;
using FluentAssertions;
using Shared.Core.Auth;
using Users.Core.Models;
using Users.Core.Models.Entities;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.AuthenticationModule;

public class RefreshTest : ApiTest
{
    public const string RequestUrl = "/api/auth/refresh";

    private User CreateTestUser() => User.Create(
        id: UserId.Create(Guid.NewGuid()).Value,
        name: UserName.Create("test").Value,
        email: Email.Create("test@gmail.com").Value,
        hashedPassword: HashedPassword.Create("12345678").Value,
        phoneNumber: PhoneNumber.Create("+380991444743").Value,
        role: UserRole.Default
    );

    public RefreshTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    [Fact]
    public async Task WhenRefreshTokenDoesNotExist_ThenReturnsBadRequest()
    {
        // Assert
        var refreshToken = "fake-refresh-token";

        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}?refreshToken={refreshToken}");

        var expectedJson =
            MakeSystemErrorApiOutput("Refresh token not found", $"Refresh token {refreshToken} not found");

        // Act 
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenRefreshTokenIsExpired_ThenReturnsBadRequest()
    {
        // Arrange
        var testUser = CreateTestUser();

        var refreshToken = RefreshToken.Create(
            "token",
            testUser.Id,
            DateTime.UtcNow.AddHours(-1)
        );

        ApplicationDbContext.Users.Add(testUser);
        ApplicationDbContext.RefreshTokens.Add(refreshToken);
        await ApplicationDbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}?refreshToken={refreshToken.Token}");

        var expectedJson = MakeSystemErrorApiOutput("Refresh token has expired",
            $"Refresh token expiration was in {refreshToken.ExpiresOnUtc}");
        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenRefreshTokenIsValid_ThenReturnsOk()
    {
        // Arrange
        var testUser = CreateTestUser(); 

        var refreshToken = RefreshToken.Create(
            "token",
            testUser.Id,
            DateTime.UtcNow.AddHours(1)
        );

        ApplicationDbContext.Users.Add(testUser);
        ApplicationDbContext.RefreshTokens.Add(refreshToken);
        await ApplicationDbContext.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{RequestUrl}?refreshToken={refreshToken.Token}");

        // Act
        var response = await HttpClient.SendAsync(request);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        actualJson.Should().Contain("refreshToken").And.Contain("accessToken");
    }
}