using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Core.Auth;
using Users.Application.Common.Interfaces;
using Users.Application.Dto.User;
using Users.Application.UseCases.Users.Commands.UpdateUser;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Users.Commands.UpdateUser;

[TestSubject(typeof(UpdateUserCommandHandler))]
public class UpdateUserCommandHandlerTest : IntegrationTest
{
    private readonly IPasswordHelper _passwordHelperMock;
    
    private const string DefaultEmail = "test@test.com";
    private const string DefaultPhoneNumber = "+380991234567";

    private User CreateDefaultUser(Guid userId, string email = DefaultEmail, string phoneNumber = DefaultPhoneNumber) => User.Create(
        id: UserId.Create(userId).Value,
        name: UserName.Create("test").Value,
        email: Email.Create(email).Value,
        phoneNumber: PhoneNumber.Create(phoneNumber).Value,
        role: UserRole.Default,
        hashedPassword: HashedPassword.Create("password").Value
    );

    private UserUpdateDto CreateDefaultUserUpdateDto() => new(
        Name: "test2",
        Email: "test2@test.com",
        PhoneNumber: "+380991234577",
        Password: "123456",
        Role: UserRole.Admin.ToString()
    );

    public UpdateUserCommandHandlerTest(DatabaseFixture dbFixture) : base(dbFixture)
    {
        _passwordHelperMock = Substitute.For<IPasswordHelper>();
    }

    [Fact]
    public async Task WhenIdIsNotInDb_UpdateUserHandle_ShouldReturnFailureResult()
    {
        // Arrange
        var user = CreateDefaultUser(Guid.NewGuid());
        var userToUpdateId = Guid.NewGuid();
        var dto = CreateDefaultUserUpdateDto();
        
        var cmd = new UpdateUserCommand(userToUpdateId, dto);
        var handler = new UpdateUserCommandHandler(_passwordHelperMock, ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new UpdateUserCommandHandler.UserNotFoundError(userToUpdateId));
    }

    [Fact]
    public async Task WhenEmailIsInDb_UpdateUser_ShouldReturnFailureResult()
    {
        // Arrange
        var userToUpdateId = Guid.NewGuid();
        var user = CreateDefaultUser(Guid.NewGuid());
        var userToUpdate = CreateDefaultUser(userToUpdateId, email: "test2@test.com");
        var dto = CreateDefaultUserUpdateDto() with {Email = DefaultEmail};
        
        var cmd = new UpdateUserCommand(userToUpdateId, dto);
        var handler = new UpdateUserCommandHandler(_passwordHelperMock, ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.Users.Add(userToUpdate);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new UpdateUserCommandHandler.IncorrectEmailError(DefaultEmail));
    }
    
    [Fact]
    public async Task WhenPhoneNumberIsInDb_UpdateUser_ShouldReturnFailureResult()
    {
        // Arrange
        var userToUpdateId = Guid.NewGuid();
        var user = CreateDefaultUser(Guid.NewGuid());
        var userToUpdate = CreateDefaultUser(userToUpdateId, phoneNumber: "+380991234577");
        var dto = CreateDefaultUserUpdateDto() with {PhoneNumber = DefaultPhoneNumber};
        
        var cmd = new UpdateUserCommand(userToUpdateId, dto);
        var handler = new UpdateUserCommandHandler(_passwordHelperMock, ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        ApplicationDbContext.Users.Add(userToUpdate);
        await ApplicationDbContext.SaveChangesAsync(default);

        // Act
        var result = await handler.Handle(cmd, default);

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e == new UpdateUserCommandHandler.IncorrectPhoneNumberError(DefaultPhoneNumber));
    }

    [Fact]
    public async Task WhenDataIsCorrect_UpdateUser_ShouldReturnSuccessResult()
    {
        // Arrange
        var userToUpdateId = Guid.NewGuid();
        var user = CreateDefaultUser(userToUpdateId);
        var dto = CreateDefaultUserUpdateDto();
        
        var cmd = new UpdateUserCommand(userToUpdateId, dto);
        var handler = new UpdateUserCommandHandler(_passwordHelperMock, ApplicationDbContext);

        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);
        
        _passwordHelperMock.HashPassword(Arg.Any<string>()).Returns(dto.Password);

        // Act
        var result = await handler.Handle(cmd, default);
        var updatedUser = ApplicationDbContext.Users.Find(UserId.Create(userToUpdateId).Value);

        // Assert
        Assert.True(result.IsSuccess);
        updatedUser.Should().Satisfy<User>(u =>
        {
            u.Name.Value.Should().Be(dto.Name);
            u.Email.Value.Should().Be(dto.Email);
            u.PhoneNumber.Value.Should().Be(dto.PhoneNumber);
            u.HashedPassword.Value.Should().Be(dto.Password);
            u.Role.Should().Be(Enum.Parse<UserRole>(dto.Role));
        });
    }
}