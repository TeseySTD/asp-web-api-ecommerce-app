using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Auth;
using Users.Application.Common.Interfaces;
using Users.Application.UseCases.Users.Commands.DeleteUser;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Users.Commands.DeleteUser;

[TestSubject(typeof(DeleteUserCommandHandler))]
public class DeleteUserCommandHandlerTest : IntegrationTest
{
    private User CreateDefaultUser(Guid userId) => User.Create(
        id: UserId.Create(userId).Value,
        name: UserName.Create("test").Value,
        email: Email.Create("test@test.com").Value,
        phoneNumber: PhoneNumber.Create("+3809912345678").Value,
        role: UserRole.Default,
        hashedPassword: HashedPassword.Create("test@test.com").Value
    );

    public DeleteUserCommandHandlerTest(DatabaseFixture dbFixture) : base(dbFixture)
    {
    }

    [Fact]
    public async Task WhenIdIsNotInDb_DeleteUserHandle_ShouldReturnFailureResult()
    {
        // Arrange 
        var user = CreateDefaultUser(Guid.NewGuid());
        var userToDeleteId = Guid.NewGuid();
        var cmd = new DeleteUserCommand(userToDeleteId);
        var handler = new DeleteUserCommandHandler(ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);

        //Act
        var result = await handler.Handle(cmd, default);
        var users = await ApplicationDbContext.Users.ToListAsync();

        //Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new DeleteUserCommandHandler.UserNotFoundError(userToDeleteId));
        users.Should().ContainSingle(u => u == user);
    }

    [Fact]
    public async Task DeleteUserWithCorrectId_ShouldReturnSuccessResult()
    {
        // Arrange 
        var userToDeleteId = Guid.NewGuid();
        var user = CreateDefaultUser(userToDeleteId);

        var cmd = new DeleteUserCommand(userToDeleteId);
        var handler = new DeleteUserCommandHandler(ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);

        //Act
        var result = await handler.Handle(cmd, default);
        var users = await ApplicationDbContext.Users.ToListAsync();

        //Assert
        Assert.True(result.IsSuccess);
        users.Should().BeEmpty();
    }
}