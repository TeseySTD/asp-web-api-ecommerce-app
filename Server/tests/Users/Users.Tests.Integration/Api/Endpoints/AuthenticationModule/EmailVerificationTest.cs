using System.Net;
using Shared.Core.Auth;
using Users.Application.UseCases.Authentication.Commands.EmailVerification;
using Users.Core.Models;
using Users.Core.Models.Entities;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.AuthenticationModule;

public class EmailVerificationTest : ApiTest
{
    public const string RequestUrl = "/api/auth/verify-email";

    public EmailVerificationTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(
        factory, databaseFixture)
    {
    }

    private User CreateTestUser() => User.Create(
        id: UserId.Create(Guid.NewGuid()).Value,
        name: UserName.Create("test").Value,
        email: Email.Create("test").Value,
        phoneNumber: PhoneNumber.Create("+380991444410").Value,
        hashedPassword: HashedPassword.Create("test").Value,
        role: UserRole.Default
    );

    [Fact]
    public async Task WhenTokenDoesNotExist_ThenReturnsBadRequest()
    {
        // Arrange
        var tokenId = Guid.NewGuid();

        var expectedContent = MakeSystemErrorApiOutput(new EmailVerificationCommandHandler.TokenNotFoundError(tokenId));

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?tokenId={tokenId}");
        var actualContent = await response.Content.ReadAsStringAsync();

        // Arrange
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenTokenIsExpired_ThenReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser();
        var token = EmailVerificationToken.Create(user.Id, DateTime.UtcNow.AddSeconds(-1));
        var tokenId = token.Id.Value;

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.EmailVerificationTokens.Add(token);
        await ApplicationDbContext.SaveChangesAsync();

        var expectedContent = MakeSystemErrorApiOutput(new EmailVerificationCommandHandler.TokenExpiredError(token.ExpiresOnUtc));

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?tokenId={tokenId}");
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenTokenIsCorrect_ThenReturnsOk()
    {
        // Arrange
        var user = CreateTestUser();
        var token = EmailVerificationToken.Create(user.Id, DateTime.UtcNow.AddSeconds(30));
        var tokenId = token.Id.Value;

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.EmailVerificationTokens.Add(token);
        await ApplicationDbContext.SaveChangesAsync();
        
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?tokenId={tokenId}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}