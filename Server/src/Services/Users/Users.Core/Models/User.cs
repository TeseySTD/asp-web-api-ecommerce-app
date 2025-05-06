using Shared.Core.Auth;
using Shared.Core.Domain.Classes;
using Users.Core.Models.Events;
using Users.Core.Models.ValueObjects;

namespace Users.Core.Models;

public class User : AggregateRoot<UserId>
{
    private User() : base(default!)
    {
    }

    private User(UserId id,
        UserName name,
        Email email,
        HashedPassword hashedPassword,
        PhoneNumber phoneNumber,
        UserRole role) : base(id)
    {
        Name = name;
        Email = email;
        HashedPassword = hashedPassword;
        PhoneNumber = phoneNumber;
        Role = role;
    }

    public UserName Name { get; private set; }
    public Email Email { get; private set; }
    public HashedPassword HashedPassword { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }

    public bool IsEmailVerified { get; private set; } = false;

    public static User Create(UserName name, Email email, HashedPassword hashedPassword, PhoneNumber phoneNumber,
        UserRole role,
        UserId? id = null)
    {
        var user = new User(id ?? UserId.Create(Guid.NewGuid()).Value, name, email, hashedPassword, phoneNumber, role);
        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
        return user;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        AddDomainEvent(new UserEmailVerifiedDomainEvent(this));
    }
    
    public void Update(UserName name, Email email, HashedPassword hashedPassword, PhoneNumber phoneNumber,
        UserRole role)
    {
        Name = name;
        Email = email;
        HashedPassword = hashedPassword;
        PhoneNumber = phoneNumber;
        Role = role;

        AddDomainEvent(new UserUpdatedDomainEvent(this));
    }
}