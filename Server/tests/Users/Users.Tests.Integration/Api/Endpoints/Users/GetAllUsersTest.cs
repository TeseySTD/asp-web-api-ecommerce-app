using System.Net;
using FluentAssertions;
using Shared.Core.Auth;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.Users;

public class GetAllUsersTest : ApiTest
{
    public const string RequestUrl = "/api/users/";

    public GetAllUsersTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(
        factory, databaseFixture)
    {
    }

    private List<User> CreateTestUsersList() => new()
    {
        User.Create(
            name: UserName.Create("John Doe").Value,
            email: Email.Create("john.doe@example.com").Value,
            phoneNumber: PhoneNumber.Create("+3809912345678").Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("12345").Value
        ),
        User.Create(
            name: UserName.Create("Maria Doe").Value,
            email: Email.Create("maria.doe@example.com").Value,
            phoneNumber: PhoneNumber.Create("+3809912345678").Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("123456").Value
        ),
        User.Create(
            name: UserName.Create("Bob Doe").Value,
            email: Email.Create("bob.doe@example.com").Value,
            phoneNumber: PhoneNumber.Create("+3809912345678").Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("1234567").Value
        )
    };

    [Fact]
    public async Task GetAllUsers_DbHasNoUsers_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync(RequestUrl);

        // Assert 
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_RequestIsOutOfRange_ReturnsNotFound()
    {
        // Arrange 
        var users = CreateTestUsersList();
        ApplicationDbContext.Users.AddRange(users);
        await ApplicationDbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}?pageIndex=1&pageSize={users.Count}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAllUsers_DataIsCorrect_ReturnsUsers()
    {
        // Arrange
        var users = CreateTestUsersList();
        ApplicationDbContext.Users.AddRange(users);
        await ApplicationDbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync( $"{RequestUrl}?pageIndex={0}");
        var json = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        foreach (var u in users)
        {
            json.Should().Contain(u.Name.Value).And
                .Contain(u.Email.Value).And
                .Contain(u.PhoneNumber.Value).And
                .Contain(u.Role.ToString());
        }
    }
}