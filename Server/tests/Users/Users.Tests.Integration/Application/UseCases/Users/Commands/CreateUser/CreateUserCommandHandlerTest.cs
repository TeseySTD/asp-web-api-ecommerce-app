using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shared.Core.Auth;
using Users.Application.Common.Interfaces;
using Users.Application.Dto.User;
using Users.Application.UseCases.Users.Commands.CreateUser;
using Users.Core.Models;
using Users.Core.Models.ValueObjects;
using Users.Tests.Integration.Common;

namespace Users.Tests.Integration.Application.UseCases.Users.Commands.CreateUser;

[TestSubject(typeof(CreateUserCommandHandler))]
public class CreateUserCommandHandlerTest : IntegrationTest
{
    private readonly IPasswordHelper _passwordHelperMock;

    private UserWriteDto CreateDefaultUserWriteDto() => new (
        Name: "test",
        Email: "test@test.com",
        Password: "test",
        PhoneNumber: "+3809912345678",
        Role: "Default"
    );
    
    private User CreateDefaultUser() => User.Create(
        name: UserName.Create("test").Value,
        email: Email.Create("test@test.com").Value,
        phoneNumber: PhoneNumber.Create("+3809912345678").Value,
        role: UserRole.Default,
        hashedPassword: HashedPassword.Create("test@test.com").Value
    );
    
    public CreateUserCommandHandlerTest(DatabaseFixture dbFixture) : base(dbFixture)
    {
        _passwordHelperMock = Substitute.For<IPasswordHelper>();
    }

    [Fact]
    public async Task WhenEmailIsNotUnique_Handle_Should_ReturnFailureResult()
    {
        // Arrange
        var dto = CreateDefaultUserWriteDto();
        var cmd = new CreateUserCommand(dto);

        var user = CreateDefaultUser();
        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);
        
        var handler = new CreateUserCommandHandler(_passwordHelperMock, ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);
        var users = await ApplicationDbContext.Users.ToListAsync();

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e.Message == "User already exists." && e.Description ==
            $"User with email: {cmd.Value.Email} already exists.");
        
        users.Should().HaveCount(1);
    }

    [Fact]
    public async Task WhenPhoneNumberIsNotUnique_Handle_Should_ReturnFailureResult()
    {
        // Arrange
        var dto = CreateDefaultUserWriteDto();
        var cmd = new CreateUserCommand(dto);

        var user = CreateDefaultUser();
        ApplicationDbContext.Users.Add(user);
        await ApplicationDbContext.SaveChangesAsync(default);
        
        var handler = new CreateUserCommandHandler(_passwordHelperMock, ApplicationDbContext);

        // Act
        var result = await handler.Handle(cmd, default);
        var users = await ApplicationDbContext.Users.ToListAsync();

        // Assert
        Assert.True(result.IsFailure);
        result.Errors.Should().ContainSingle(e => e.Message == "User already exists." && e.Description ==
            $"User with number: {cmd.Value.PhoneNumber} already exists.");
        
        users.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreateUser_WithCorrectData_Should_ReturnSuccessResult()
    {
        // Arrange
        var dto = CreateDefaultUserWriteDto();
        var cmd = new CreateUserCommand(dto);
        
        _passwordHelperMock.HashPassword(Arg.Any<string>()).Returns(dto.Password);
        
        var handler = new CreateUserCommandHandler(_passwordHelperMock, ApplicationDbContext);
        
        // Act 
        var result = await handler.Handle(cmd, default);
        var users = await ApplicationDbContext.Users.ToListAsync();
        
        // Assert
        Assert.True(result.IsSuccess);
        
        result.Value.Should().Satisfy<User>(v =>
        {
            v.Name.Value.Should().Be(dto.Name);
            v.Email.Value.Should().Be(dto.Email);
            v.PhoneNumber.Value.Should().Be(dto.PhoneNumber);
            v.HashedPassword.Value.Should().Be(dto.Password);
            v.Role.ToString().Should().Be(dto.Role);
        });
        
        users.Should().Contain(u => u == result.Value);
    }
}