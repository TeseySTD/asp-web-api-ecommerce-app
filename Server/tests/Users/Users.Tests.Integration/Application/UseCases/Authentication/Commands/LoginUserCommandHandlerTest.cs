using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Core.Auth;
using Users.Application.Common.Interfaces;
using Users.Application.UseCases.Authentication.Commands.Login;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Authentication.Commands;

[TestSubject(typeof(LoginUserCommandHandler))]
public class LoginUserCommandHandlerTest : IntegrationTest
{
    private readonly IPasswordHelper _passwordHelperMock;
    private readonly ITokenProvider _tokenProviderMock;

    public LoginUserCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _tokenProviderMock = Substitute.For<ITokenProvider>();
        _passwordHelperMock = Substitute.For<IPasswordHelper>();
    }

    private User CreateTestUser(string email, string password) => User.Create(
        name: UserName.Create("test").Value,
        email: Email.Create(email).Value,
        hashedPassword: HashedPassword.Create(password).Value,
        phoneNumber: PhoneNumber.Create("+380991444743").Value,
        role: UserRole.Default
    );
    
    [Fact]
    public async Task Handle_UserWithEmailIsNotInDb_ReturnsEmailNotFoundError()
    {
        // Arrange
        var email = "test@test.com";
        var cmd = new LoginUserCommand(email, "password");
        var handler = new LoginUserCommandHandler(_tokenProviderMock, _passwordHelperMock, ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new LoginUserCommandHandler.EmailNotFoundError(email));
    }

    [Fact]
    public async Task Handle_PasswordIsNotOfUser_ReturnsIncorrectPasswordError()
    {
        // Arrange
        var email = "test@test.com";
        var password = "password";
        var userPassword = "userPassword";
        var user = CreateTestUser(email, password);

        _passwordHelperMock.VerifyPassword(userPassword, password).Returns(false);

        var cmd = new LoginUserCommand(email, password);
        var handler = new LoginUserCommandHandler(_tokenProviderMock, _passwordHelperMock, ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should()
            .ContainSingle(e => e == new LoginUserCommandHandler.IncorrectPasswordError(email, password));
    }

    [Fact]
    public async Task Handle_DataIsCorrect_ReturnsSuccessResult()
    {
        // Arrange
        var email = "test@test.com";
        var password = "password";
        var user = CreateTestUser(email, password);
        var refreshToken = Core.Models.Entities.RefreshToken.Create(
            token: "refreshToken",
            userId: user.Id,
            expiresOnUtc: DateTime.UtcNow.AddDays(7)
        );
        var jwt = "jwtToken";

        _passwordHelperMock.VerifyPassword(user.HashedPassword.Value, password).Returns(true);
        _tokenProviderMock.GenerateRefreshToken(Arg.Any<User>()).Returns(refreshToken);
        _tokenProviderMock.GenerateJwtToken(Arg.Any<User>()).Returns(jwt);

        var cmd = new LoginUserCommand(email, password);
        var handler = new LoginUserCommandHandler(_tokenProviderMock, _passwordHelperMock, ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);
        var isRefeshTokenInDb = await ApplicationDbContext.RefreshTokens.AnyAsync(r => r.Id == refreshToken.Id);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(isRefeshTokenInDb);
        result.Value.AccessToken.Should().Be(jwt);
        result.Value.RefreshToken.Should().Be(refreshToken.Token);
    }
}