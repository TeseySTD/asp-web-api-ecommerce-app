using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using Shared.Core.Auth;
using Users.Application.Common.Interfaces;
using Users.Application.Dto.User;
using Users.Application.UseCases.Users.Commands.UpdateUser;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Users.Commands;

[TestSubject(typeof(UpdateUserCommandHandler))]
public class UpdateUserCommandHandlerTest : IntegrationTest
{
    private readonly IPasswordHelper _passwordHelperMock;
    
    private const string DefaultEmail = "test@test.com";
    private const string DefaultPhoneNumber = "+380991234567";

    private User CreateTestUser(Guid userId, string email = DefaultEmail, string phoneNumber = DefaultPhoneNumber) => User.Create(
        id: UserId.Create(userId).Value,
        name: UserName.Create("test").Value,
        email: Email.Create(email).Value,
        phoneNumber: PhoneNumber.Create(phoneNumber).Value,
        role: UserRole.Default,
        hashedPassword: HashedPassword.Create("password").Value
    );

    private UserUpdateDto CreateTestUserUpdateDto() => new(
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
    public async Task Handle_IdIsNotInDb_ReturnsUserNotFoundError()
    {
        // Arrange
        var user = CreateTestUser(Guid.NewGuid());
        var userToUpdateId = Guid.NewGuid();
        var dto = CreateTestUserUpdateDto();
        
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
    public async Task Handle_EmailIsInDb_ReturnsIncorrectEmailError()
    {
        // Arrange
        var userToUpdateId = Guid.NewGuid();
        var user = CreateTestUser(Guid.NewGuid());
        var userToUpdate = CreateTestUser(userToUpdateId, email: "test2@test.com");
        var dto = CreateTestUserUpdateDto() with {Email = DefaultEmail};
        
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
    public async Task Handle_PhoneNumberIsInDb_ReturnsIncorrectPhoneNumberError()
    {
        // Arrange
        var userToUpdateId = Guid.NewGuid();
        var user = CreateTestUser(Guid.NewGuid());
        var userToUpdate = CreateTestUser(userToUpdateId, phoneNumber: "+380991234577");
        var dto = CreateTestUserUpdateDto() with {PhoneNumber = DefaultPhoneNumber};
        
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
    public async Task Handle_DataIsCorrect_ReturnSuccessResult()
    {
        // Arrange
        var userToUpdateId = Guid.NewGuid();
        var user = CreateTestUser(userToUpdateId);
        var dto = CreateTestUserUpdateDto();
        
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