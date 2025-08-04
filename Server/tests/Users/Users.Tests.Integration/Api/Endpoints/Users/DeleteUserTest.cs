using System.Net;
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

    [Fact]
    public async Task WhenUserIsNotInDb_ThenReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var expectedContent = MakeSystemErrorApiOutput(new DeleteUserCommandHandler.UserNotFoundError(userId));

        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userId}");
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenUserIsInDb_ThenReturnsOk()
    {
        // Arrange 
        var userToDelete = User.Create(
            id: UserId.Create(Guid.NewGuid()).Value,
            name: UserName.Create("test").Value,
            email: Email.Create("test@test.com").Value,
            phoneNumber: PhoneNumber.Create("+380991234567").Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("test@test.com").Value
        );
        
        ApplicationDbContext.Users.Add(userToDelete);
        await ApplicationDbContext.SaveChangesAsync();
        
        // Act
        var response = await HttpClient.DeleteAsync($"{RequestUrl}/{userToDelete.Id.Value}");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}