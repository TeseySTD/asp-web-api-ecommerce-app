using System.Net;
using System.Text;
using Newtonsoft.Json;
using Shared.Core.Auth;
using Users.API.Http.User.Requests;
using Users.Application.UseCases.Users.Commands.UpdateUser;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.Users;

public class UpdateUserTest : ApiTest
{
    public const string RequestUrl = "/api/users";

    public UpdateUserTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(factory,
        databaseFixture)
    {
    }

    private const string TestEmail = "test@test.com";
    private const string TestPhoneNumber = "+380991234567";

    private User CreateTestUser(Guid userId, string email = TestEmail, string phoneNumber = TestPhoneNumber) =>
        User.Create(
            id: UserId.Create(userId).Value,
            name: UserName.Create("test").Value,
            email: Email.Create(email).Value,
            phoneNumber: PhoneNumber.Create(phoneNumber).Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("password").Value
        );

    private StringContent GenerateRequestBody(User user, string email = "",
        string phoneNumber = "")
    {
        StringContent stringContent =
            new StringContent(
                JsonConvert.SerializeObject(new UpdateUserRequest
                    (
                        user.Name.Value,
                        email == "" ? user.Email.Value : email,
                        "password",
                        phoneNumber == "" ? user.PhoneNumber.Value : phoneNumber,
                        UserRole.Default.ToString()
                    )
                ),
                Encoding.UTF8,
                "application/json"
            );
        return stringContent;
    }

    private StringContent GenerateRequestBody() => GenerateRequestBody(CreateTestUser(Guid.NewGuid()));

    [Fact]
    public async Task WhenIdIsNotFound_ThenReturnsNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var expectedContent = MakeSystemErrorApiOutput(new UpdateUserCommandHandler.UserNotFoundError(userId));

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{userId}", GenerateRequestBody());
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenEmailIsAlreadySet_ThenReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser(Guid.NewGuid());
        var userToUpdateNumber = "+380991234577";
        var userToUpdate = CreateTestUser(Guid.NewGuid(), "test2@test.com", userToUpdateNumber);
        var newEmail = TestEmail;

        ApplicationDbContext.Users.Add(userToUpdate);
        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync();

        var content = GenerateRequestBody(userToUpdate, email: newEmail);

        var expectedContent = MakeSystemErrorApiOutput(new UpdateUserCommandHandler.IncorrectEmailError(newEmail));

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{userToUpdate.Id.Value}", content);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenPhoneNumberIsAlreadySet_ThenReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser(Guid.NewGuid());
        var userToUpdateEmail = "test2@test.com";
        var userToUpdate = CreateTestUser(Guid.NewGuid(), userToUpdateEmail, "+380991234577");
        var newPhoneNumber = TestPhoneNumber;

        ApplicationDbContext.Users.AddRange(userToUpdate, user);
        await ApplicationDbContext.SaveChangesAsync();

        var content = GenerateRequestBody(userToUpdate, phoneNumber: newPhoneNumber);

        var expectedContent = MakeSystemErrorApiOutput(new UpdateUserCommandHandler.IncorrectPhoneNumberError(newPhoneNumber));

        // Act
        var response = await HttpClient.PutAsync( $"{RequestUrl}/{userToUpdate.Id.Value}", content);
        var actualContent = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(expectedContent, actualContent, ignoreAllWhiteSpace: true);
    }

    [Fact]
    public async Task WhenUpdateDataIsCorrect_ThenReturnsOk()
    {
        // Arrange
        var userToUpdate = CreateTestUser(Guid.NewGuid());
        var newEmail = "test2@test.com";
        
        ApplicationDbContext.Users.Add(userToUpdate);
        await ApplicationDbContext.SaveChangesAsync();  

        var content = GenerateRequestBody(userToUpdate, email: newEmail);

        // Act
        var response = await HttpClient.PutAsync($"{RequestUrl}/{userToUpdate.Id.Value}", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}