using FluentAssertions;
using Shared.Core.Auth;
using Users.Core.Models;
using Users.Core.Models.Events;
using Users.Core.Models.ValueObjects;

namespace Users.Core.Tests.Models;

public class UserTest
{
    private readonly UserId _defaultUserId = UserId.Create(Guid.NewGuid()).Value;

    private User CreateDefaultUser() => User.Create(
        name: UserName.Create("User").Value,
        email: Email.Create("User@gmail.com").Value,
        hashedPassword: HashedPassword.Create("awdawdawdaw").Value,
        phoneNumber: PhoneNumber.Create("+38099114430").Value,
        role: UserRole.Default,
        id: _defaultUserId
    );

    [Fact]
    public void VerifyEmail_WhenCalled_SetsIsEmailVerifiedAndAddsDomainEvent()
    {
        // Arrange
        var user = CreateDefaultUser();

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailVerified.Should().BeTrue();
        user.DomainEvents.OfType<UserEmailVerifiedDomainEvent>().Should().ContainSingle();
    }

    [Fact]
    public void UpdateUser_WhenCalled_UpdatesPropertiesAndAddsDomainEvent()
    {
        // Arrange
        var user = CreateDefaultUser();
        var newName = UserName.Create("NewName").Value;
        var newEmail = Email.Create("NewEmail@gmail.com").Value;
        var newHashedPassword = HashedPassword.Create("awdawdawdaw2").Value;
        var newPhoneNumber = PhoneNumber.Create("+38099114431").Value;
        var newRole = UserRole.Admin;

        // Act 
        user.Update(newName, newEmail, newHashedPassword, newPhoneNumber, newRole);

        // Assert
        user.Name.Should().Be(newName);
        user.Email.Should().Be(newEmail);
        user.HashedPassword.Should().Be(newHashedPassword);
        user.PhoneNumber.Should().Be(newPhoneNumber);
        user.Role.Should().Be(newRole);

        user.DomainEvents.OfType<UserUpdatedDomainEvent>().Should().ContainSingle();
    }
}