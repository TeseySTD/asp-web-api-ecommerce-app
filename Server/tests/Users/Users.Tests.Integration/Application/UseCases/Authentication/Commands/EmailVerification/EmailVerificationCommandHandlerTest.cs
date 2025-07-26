using FluentAssertions;
using JetBrains.Annotations;
using Shared.Core.Auth;
using Users.Application.UseCases.Authentication.Commands.EmailVerification;
using Users.Core.Models;
using Users.Core.Models.Entities;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Authentication.Commands.EmailVerification;

[TestSubject(typeof(EmailVerificationCommandHandler))]
public class EmailVerificationCommandHandlerTest : IntegrationTest
{
    private UserId _defaultUserId = UserId.Create(Guid.NewGuid()).Value;

    private EmailVerificationToken CreateDefaultEmailVerificationToken(DateTime expiresOnUtc) =>
        EmailVerificationToken.Create(
            _defaultUserId,
            expiresOnUtc
        );
    private User CreateDefaultUser() => User.Create(
        id: _defaultUserId,
        name: UserName.Create("test").Value,
        email: Email.Create("test").Value,
        phoneNumber: PhoneNumber.Create("+380991444410").Value,
        hashedPassword: HashedPassword.Create("test").Value,
        role: UserRole.Default
    );

    public EmailVerificationCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task WhenEmailVerificationTokenIsNotInDb_Handler_ReturnFailureResult()
    {
        // Arrange
        var tokenId = Guid.NewGuid();
        var cmd = new EmailVerificationCommand(tokenId);
        var handler = new EmailVerificationCommandHandler(ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);

        // Arrange
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new EmailVerificationCommandHandler.TokenNotFoundError(tokenId));
    }

    [Fact]
    public async Task WhenEmailVerificationTokenExpired_Handler_ReturnFailureResult()
    {
        // Arrange
        var user = CreateDefaultUser();
        var token = CreateDefaultEmailVerificationToken(DateTime.UtcNow.AddSeconds(-1));
        var tokenId = token.Id;
        var cmd = new EmailVerificationCommand(tokenId.Value);
        var handler = new EmailVerificationCommandHandler(ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.EmailVerificationTokens.Add(token);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().Contain(new EmailVerificationCommandHandler.TokenExpiredError(token.ExpiresOnUtc));
    }

    [Fact]
    public async Task WhenDataCorrect_Handler_ReturnSuccessResult()
    {
        // Arrange
        var user = CreateDefaultUser();
        var token = CreateDefaultEmailVerificationToken(DateTime.UtcNow.AddMinutes(10));
        var tokenId = token.Id;
        var cmd = new EmailVerificationCommand(tokenId.Value);
        var handler = new EmailVerificationCommandHandler(ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.EmailVerificationTokens.Add(token);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);
        user = await ApplicationDbContext.Users.FindAsync(user.Id);
        token = await ApplicationDbContext.EmailVerificationTokens.FindAsync(tokenId);

        // Assert
        Assert.True(result.IsSuccess);
        token.Should().BeNull();
        Assert.True(user!.IsEmailVerified);
    }
}