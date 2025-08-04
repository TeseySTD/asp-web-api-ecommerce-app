using System.Net;
using FluentAssertions;
using Shared.Core.Auth;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Api.Endpoints.Users;

public class GetUserByIdTest : ApiTest
{
    public const string RequestUrl = "/api/users";

    public GetUserByIdTest(IntegrationTestWebApplicationFactory factory, DatabaseFixture databaseFixture) : base(
        factory, databaseFixture)
    {
    }

    [Fact]
    public async Task WhenUserWithIdIsNotFound_ThenReturnsNotFound()
    {
        // Arrange 
        var userId = Guid.NewGuid();

        // Act
        var response = await HttpClient.GetAsync( $"{RequestUrl}/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task WhenUserWithIdIsFound_ThenReturnsUser()
    {
        // Arrange
        var user = User.Create(
            id: UserId.Create(Guid.NewGuid()).Value,
            name: UserName.Create("test").Value,
            email: Email.Create("test@test.com").Value,
            phoneNumber: PhoneNumber.Create("+3809912345678").Value,
            role: UserRole.Default,
            hashedPassword: HashedPassword.Create("test@test.com").Value
        );
        
        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync();
        
        // Act
        var response = await HttpClient.GetAsync($"{RequestUrl}/{user.Id.Value}");
        var json = await response.Content.ReadAsStringAsync();
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        json.Should().Contain(user.Name.Value).And
            .Contain(user.Email.Value).And
            .Contain(user.PhoneNumber.Value).And
            .Contain(user.Role.ToString());
    }
}