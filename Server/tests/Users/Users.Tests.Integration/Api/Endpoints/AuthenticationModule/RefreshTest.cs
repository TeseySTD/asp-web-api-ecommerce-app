using System.Net;
using FluentAssertions;
using Shared.Core.Auth;
using Users.Application.UseCases.Authentication.Commands.RefreshToken;
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
    public async Task Refresh_TokenDoesNotExist_ReturnsBadRequest()
    {
        // Assert
        var refreshToken = "fake-refresh-token";

        var expectedJson =
            MakeSystemErrorApiOutput(
                new RefreshTokenCommandHandler.TokenNotFoundError(refreshToken));

        // Act 
        var response = await HttpClient.PostAsync($"{RequestUrl}?refreshToken={refreshToken}", new StringContent(""));
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task Refresh_TokenIsExpired_ReturnsBadRequest()
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

        var expectedJson =
            MakeSystemErrorApiOutput(
                new RefreshTokenCommandHandler.TokenExpiredError(refreshToken.ExpiresOnUtc));
        // Act
        var response = await HttpClient.PostAsync($"{RequestUrl}?refreshToken={refreshToken.Token}", new StringContent(""));
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task Refresh_TokenIsValid_ReturnsOk()
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

        // Act
        var response = await HttpClient.PostAsync($"{RequestUrl}?refreshToken={refreshToken.Token}", new StringContent(""));
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        actualJson.Should().Contain("refreshToken").And.Contain("accessToken");
    }
}