using System.Net;
using System.Text;
using System.Text.Json;
using Shared.Core.Auth;
using Shared.Core.Validation.Result;
using Users.API.Http.Auth.Requests;
using Users.Application.UseCases.Authentication.Commands.Login;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.AuthenticationModule;

public class LoginTest : ApiTest
{
    public const string RequestUrl = "/api/auth/login";

    public LoginTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private User CreateTestUser(string email, string password) =>
        User.Create(
            id: UserId.Create(Guid.NewGuid()).Value,
            name: UserName.Create("test").Value,
            email: Email.Create(email).Value,
            phoneNumber: PhoneNumber.Create("+380991444230").Value,
            hashedPassword: HashedPassword.Create(PasswordHelper.HashPassword(password)).Value,
            role: UserRole.Default
        );

    [Fact]
    public async Task Login_UserIsNotRegistered_BadRequestIsReturned()
    {
        // Arrange
        var userEmail = "test@test.com";
        var userPassword = "test test test";
        
        var json = JsonSerializer.Serialize(new LoginUserRequest(userEmail, userPassword));
        using StringContent content = new(json, Encoding.UTF8, "application/json");

        var expectedJson = MakeSystemErrorApiOutput(new LoginUserCommandHandler.EmailNotFoundError(userEmail));

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Login_PasswordIsNotOfUser_BadRequestIsReturned()
    {
        // Arrange
        var userEmail = "test@test.com";
        var userPassword = "test test test";
        var userIncorrectPassword = "test test test2";

        var user = CreateTestUser(userEmail, userPassword);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync();

        var json = JsonSerializer.Serialize(new LoginUserRequest(userEmail, userIncorrectPassword));
        using StringContent content = new(json, Encoding.UTF8, "application/json");

        var expectedJson =
            MakeSystemErrorApiOutput(
                new LoginUserCommandHandler.IncorrectPasswordError(userEmail, userIncorrectPassword));

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);
        var actualJson = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedJson, actualJson, ignoreAllWhiteSpace: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Login_CredentialsAreCorrecct_OkIsReturned()
    {
        // Arrange
        var userEmail = "test@test.com";
        var userPassword = "test test test";

        var user = CreateTestUser(userEmail, userPassword);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync();

        var json = JsonSerializer.Serialize(new LoginUserRequest(userEmail, userPassword));
        using StringContent content = new(json, Encoding.UTF8, "application/json");

        // Act
        var response = await HttpClient.PostAsync(RequestUrl, content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}