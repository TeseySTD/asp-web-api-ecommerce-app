using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Core.Auth;
using Users.Application.Common.Interfaces;
using Users.Application.UseCases.Authentication.Commands.RefreshToken;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Authentication.Commands;

[TestSubject(typeof(RefreshTokenCommandHandler))]
public class RefreshTokenCommandHandlerTest : IntegrationTest
{
    private ITokenProvider _tokenProviderMock;

    private User CreateTestUser(UserId id) => User.Create(
        id: id,
        name: UserName.Create("test").Value,
        email: Email.Create("test@gmail.com").Value,
        hashedPassword: HashedPassword.Create("12345678").Value,
        phoneNumber: PhoneNumber.Create("+380991444743").Value,
        role: UserRole.Default
    );

    public RefreshTokenCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _tokenProviderMock = Substitute.For<ITokenProvider>();
    }

    [Fact]
    public async Task Handle_RefreshTokenIsNotInDb_ReturnsTokenNotFoundError()
    {
        // Arrange
        var refreshTokenString = "refreshToken";
        var cmd = new RefreshTokenCommand(refreshTokenString);
        var handler = new RefreshTokenCommandHandler(_tokenProviderMock, ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new RefreshTokenCommandHandler.TokenNotFoundError(refreshTokenString));
    }

    [Fact]
    public async Task Handle_RefreshTokenIsExpired_ReturnsTokenExpiredError()
    {
        // Arrange
        var userId = UserId.Create(Guid.NewGuid()).Value;
        var user = CreateTestUser(userId);
        var refreshToken = Core.Models.Entities.RefreshToken.Create(
            "refreshToken",
            userId,
            DateTime.UtcNow.AddSeconds(-1)
        );
        var cmd = new RefreshTokenCommand(refreshToken.Token);
        var handler = new RefreshTokenCommandHandler(_tokenProviderMock, ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.RefreshTokens.Add(refreshToken);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new RefreshTokenCommandHandler.TokenExpiredError(refreshToken.ExpiresOnUtc));
    }

    [Fact]
    public async Task Handle_RefreshTokenIsValid_ReturnsSuccessResult()
    {
        // Arrange
        var userId = UserId.Create(Guid.NewGuid()).Value;
        var user = CreateTestUser(userId);
        var refreshToken = Core.Models.Entities.RefreshToken.Create(
            "refreshToken",
            userId,
            DateTime.UtcNow.AddMinutes(10)
        );
        var newRefreshToken = Core.Models.Entities.RefreshToken.Create(
            "newRefreshToken",
            userId,
            DateTime.UtcNow.AddMinutes(10)
        );
        var jwt = "jwtToken";

        _tokenProviderMock.GenerateRefreshToken(Arg.Any<User>()).Returns(newRefreshToken);
        _tokenProviderMock.GenerateJwtToken(Arg.Any<User>()).Returns(jwt);

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.RefreshTokens.Add(refreshToken);
        await ApplicationDbContext.SaveChangesAsync(default);

        var cmd = new RefreshTokenCommand(refreshToken.Token);
        var handler = new RefreshTokenCommandHandler(_tokenProviderMock, ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);
        var isPreviousRefreshTokenInDb =
            await ApplicationDbContext.RefreshTokens.AnyAsync(t => t.Token == refreshToken.Token);
        var isNewRefreshTokenInDb =
            await ApplicationDbContext.RefreshTokens.AnyAsync(t => t.Token == result.Value.RefreshToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(isPreviousRefreshTokenInDb);
        Assert.True(isNewRefreshTokenInDb);
        result.Value.RefreshToken.Should().Be(newRefreshToken.Token);
        result.Value.AccessToken.Should().Be(jwt);
    }
}