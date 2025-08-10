using FluentAssertions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Core.Auth;
using Shared.Core.Validation.Result;
using Users.Application.Common.Interfaces;
using Users.Application.Dto.User;
using Users.Application.UseCases.Authentication.Commands.Register;
using Users.Application.UseCases.Users.Commands.CreateUser;
using Users.Core.Models;
using Users.Core.Models.Entities;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Authentication.Commands;

[TestSubject(typeof(RegisterUserCommandHandler))]
public class RegisterUserCommandHandlerTest : IntegrationTest
{
    private readonly ISender _senderMock;
    private readonly ITokenProvider _tokenProviderMock;
    private readonly IEmailHelper _emailHelperMock;
    private readonly IEmailVerificationLinkFactory _emailVerificationLinkFactoryMock;
    
    private readonly string _jwtTokenDefault = "jwtToken";
    private readonly string _refreshTokenStringDefault = "refreshToken";
    
    private UserWriteDto CreateUserWriteDto() => new (
        Name: "Name",
        Email: "email@email.com",
        Password: "Password",
        PhoneNumber: "+380991234567",
        Role: "Default"
    );
    
    private User CreateUserFromDto(UserWriteDto dto) => User.Create(
        name: UserName.Create(dto.Name).Value,
        email: Email.Create(dto.Email).Value,
        hashedPassword: HashedPassword.Create(dto.Password).Value,
        phoneNumber: PhoneNumber.Create(dto.PhoneNumber).Value,
        role: Enum.Parse<UserRole>(dto.Role)
    );

    private RegisterUserCommandHandler CreateRegisterUserCommandHandler() => new(
        _senderMock,
        _tokenProviderMock,
        ApplicationDbContext,
        _emailHelperMock,
        _emailVerificationLinkFactoryMock
    );

    public RegisterUserCommandHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _senderMock = Substitute.For<ISender>();
        _tokenProviderMock = Substitute.For<ITokenProvider>();
        _tokenProviderMock.GenerateRefreshToken(Arg.Any<User>()).Returns(u =>
            RefreshToken.Create(
                token: _refreshTokenStringDefault,
                ((User)u[0]).Id,
                DateTime.UtcNow.AddMinutes(15) 
            )
        );
        _tokenProviderMock.GenerateJwtToken(Arg.Any<User>()).Returns(_jwtTokenDefault);
        _tokenProviderMock.GenerateEmailVerificationToken(Arg.Any<User>()).Returns(u =>
            EmailVerificationToken.Create(
                ((User)u[0]).Id,
                DateTime.UtcNow.AddMinutes(15) 
            )
        );
        _emailHelperMock = Substitute.For<IEmailHelper>();
        _emailVerificationLinkFactoryMock = Substitute.For<IEmailVerificationLinkFactory>();
    }

    [Fact]
    public async Task Handle_CreateUserFails_ReturnsFailureResult()
    {
        // Arrange
        var dto = CreateUserWriteDto();
        var cmd = new RegisterUserCommand(dto);
        var handler = CreateRegisterUserCommandHandler();

        var userErrors = new[] { new Error("Foo", "Bar") };
        _senderMock.Send(Arg.Any<CreateUserCommand>()).Returns(Result<User>.Failure(userErrors));

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEquivalentTo(userErrors);
        await _emailHelperMock.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_LinkFactoryFails_ShouldDoesNotSendEmailOrSaveTokensAndReturnFailureResult()
    {
        // Arrange
        var dto = CreateUserWriteDto();
        var userToCreate = CreateUserFromDto(dto);
        var cmd = new RegisterUserCommand(dto);
        var handler = CreateRegisterUserCommandHandler();

        _senderMock.Send(Arg.Any<CreateUserCommand>()).Returns(Result<User>.Success(userToCreate));

        var emailVerificationErrors = new[] { new Error("Foo", "Bar") };
        _emailVerificationLinkFactoryMock.Create(Arg.Any<EmailVerificationToken>())
            .Returns(Result<string>.Failure(emailVerificationErrors));
        
        ApplicationDbContext.Users.Add(userToCreate);
        await ApplicationDbContext.SaveChangesAsync(default);
        
        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);
        var isRefreshTokensNotEmpty = await ApplicationDbContext.RefreshTokens.AnyAsync();
        var isEmailVerificationTokensNotEmpty = await ApplicationDbContext.EmailVerificationTokens.AnyAsync();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().BeEquivalentTo(emailVerificationErrors);
        isRefreshTokensNotEmpty.Should().BeTrue();
        isEmailVerificationTokensNotEmpty.Should().BeTrue();
        await _emailHelperMock.DidNotReceive().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_AllServicesWorks_SavesTokensSendsEmailAndReturnsDtoSuccessResult()
    {
        // Arrange
        var dto = CreateUserWriteDto();
        var userToCreate = CreateUserFromDto(dto);
        var cmd = new RegisterUserCommand(dto);
        var handler = CreateRegisterUserCommandHandler();

        _senderMock.Send(Arg.Any<CreateUserCommand>()).Returns(Result<User>.Success(userToCreate));
        _emailVerificationLinkFactoryMock.Create(Arg.Any<EmailVerificationToken>())
            .Returns(Result<string>.Success(default!));
        
        ApplicationDbContext.Users.Add(userToCreate);
        await ApplicationDbContext.SaveChangesAsync(default);
        
        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);
        var isRefreshTokensNotEmpty = await ApplicationDbContext.RefreshTokens.AnyAsync();
        var isEmailVerificationTokensNotEmpty = await ApplicationDbContext.EmailVerificationTokens.AnyAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(_jwtTokenDefault);
        result.Value.RefreshToken.Should().Be(_refreshTokenStringDefault);
        isRefreshTokensNotEmpty.Should().BeTrue();
        isEmailVerificationTokensNotEmpty.Should().BeTrue();
        await _emailHelperMock.Received().SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
    }
}