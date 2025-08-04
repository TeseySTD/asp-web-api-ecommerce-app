using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Auth;
using Users.Application.UseCases.Authentication.Commands.Logout;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Authentication.Commands;

[TestSubject(typeof(LogoutUserCommandHandler))]
public class LogoutUserCommandHandlerTest : IntegrationTest
{
    public LogoutUserCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task WhenUserIsNotInDb_ThenReturnsFailureResult()
    {
        // Arrange 
        var userId = Guid.NewGuid();
        var cmd = new LogoutUserCommand(userId);
        var handler = new LogoutUserCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e =>
            e == new LogoutUserCommandHandler.UserNotFoundError(userId));
    }

    [Fact]
    public async Task WhenDataIsCorrect_ThenReturnsSuccessResult()
    {
        // Arrange
        var user = User.Create(
            name: UserName.Create("test").Value,
            email: Email.Create("test@gmail.com").Value,
            hashedPassword: HashedPassword.Create("12345678").Value,
            phoneNumber: PhoneNumber.Create("+380991444743").Value,
            role: UserRole.Default
        );
        var refreshToken = Core.Models.Entities.RefreshToken.Create(
            token: "refreshToken",
            userId: user.Id,
            expiresOnUtc: DateTime.UtcNow.AddDays(7)
        );

        var cmd = new LogoutUserCommand(user.Id.Value);
        var handler = new LogoutUserCommandHandler(ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.RefreshTokens.Add(refreshToken);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);
        var isRefreshTokenInDb = await ApplicationDbContext.RefreshTokens.AnyAsync(t => t.Id == refreshToken.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(isRefreshTokenInDb);
    }
}