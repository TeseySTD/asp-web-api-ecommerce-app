using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.API;
using Shared.Core.Auth;
using Users.Application.Common.Interfaces;
using Users.Application.UseCases.Users.Queries.GetUsers;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Users.Queries.GetUsers;

[TestSubject(typeof(GetUsersQueryHandler))]
public class GetUsersQueryHandlerTest : IntegrationTest
{
    private List<User> CreateDefaultUsersList() => new()
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

    public GetUsersQueryHandlerTest(DatabaseFixture dbFixture) : base(dbFixture)
    {
    }

    [Fact]
    public async Task WhenDbHasNoUsers_QueryHandler_ReturnsFailureResult()
    {
        // Arrange
        var query = new GetUsersQuery(new PaginationRequest());
        var handler = new GetUsersQueryHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e =>
            e.Message == "Users not found" && e.Description == "There is no users in the database.");
    }

    [Fact]
    public async Task WhenQueryIsOutOfRange_QueryHandler_ReturnsFailureResult()
    {
        // Arrange
        var users = CreateDefaultUsersList();
        var query = new GetUsersQuery(new PaginationRequest(1, users.Count));
        var handler = new GetUsersQueryHandler(ApplicationDbContext);

        ApplicationDbContext.Users.AddRange(users);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e =>
            e.Message == "Users not found" && e.Description == "There is no users in the database.");
    }

    [Fact]
    public async Task WhenDbHasUsers_QueryHandler_ReturnsSuccessResult()
    {
        // Arrange
        var users = CreateDefaultUsersList();
        var query = new GetUsersQuery(new PaginationRequest(0, users.Count));
        var handler = new GetUsersQueryHandler(ApplicationDbContext);

        ApplicationDbContext.Users.AddRange(users);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);

        result.Value.Data.Should().HaveCount(users.Count);
        result.Value.Data.Should().AllSatisfy(dto =>
        {
            users.Should().Contain(u =>
                dto.Id == u.Id.Value &&
                dto.Name == u.Name.Value &&
                dto.Email == u.Email.Value &&
                dto.PhoneNumber == u.PhoneNumber.Value &&
                dto.Role == u.Role.ToString()
            );
        });
    }

    [Fact]
    public async Task WhenQueryAskLimitedCount_QueryHandler_ReturnsSuccessResult()
    {
        // Arrange
        var users = CreateDefaultUsersList();
        var query = new GetUsersQuery(new PaginationRequest(0, users.Count - 1));
        var handler = new GetUsersQueryHandler(ApplicationDbContext);

        ApplicationDbContext.Users.AddRange(users);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(query, default);

        // Assert
        Assert.True(result.IsSuccess);

        result.Value.Data.Should().HaveCount(users.Count - 1);
        result.Value.Data.Should().AllSatisfy(dto =>
        {
            users.Should().Contain(u =>
                dto.Id == u.Id.Value &&
                dto.Name == u.Name.Value &&
                dto.Email == u.Email.Value &&
                dto.PhoneNumber == u.PhoneNumber.Value &&
                dto.Role == u.Role.ToString()
            );
        });
    }
}