using System.Net;
using System.Net.Http.Headers;
using Shared.Core.Auth;
using Users.Application.UseCases.Users.Commands.DeleteUser;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.Users;

public class DeleteUserTest : ApiTest
{
    public const string RequestUrl = "/api/users";

    public DeleteUserTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private User CreateTestUser(Guid userId) =>
        User.Create(
            id: UserId.Create(userId).Value,
            name: UserName.Create("test").Value,
            email: Email.Create("test@gmail.com").Value,
            phoneNumber: PhoneNumber.Create("+380991234567").Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("password").Value
        );

    [Fact]
    public async Task DeleteUser_Unathorized_ReturnsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_IdIsNotBelongToUser_ReturnsForbidden()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = CreateTestUser(Guid.NewGuid()); // User with other id

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenProvider.GenerateJwtToken(testUser));
        
        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_UserIsNotInDb_ReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var testUser = CreateTestUser(userId);

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenProvider.GenerateJwtToken(testUser));

        var expectedContent = MakeSystemErrorApiOutput(new DeleteUserCommandHandler.UserNotFoundError(userId));

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userId}");
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task DeleteUser_UserIsInDb_ReturnsOk()
    {
        // Arrange 
        var userToDelete = CreateTestUser(Guid.NewGuid());

        ApplicationDbContext.Users.Add(userToDelete);
        await ApplicationDbContext.SaveChangesAsync();

        HttpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", TokenProvider.GenerateJwtToken(userToDelete));

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userToDelete.Id.Value}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}