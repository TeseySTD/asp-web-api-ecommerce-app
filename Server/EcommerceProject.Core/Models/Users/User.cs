using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Products.Events;
using EcommerceProject.Core.Models.Users.Events;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Core.Models.Users;

public class User : AggregateRoot<UserId>
{
    private User() : base(default!)
    {
    }

    private User(UserId id, UserName name, Email email, Password password, PhoneNumber phoneNumber,
        UserRole role) : base(id)
    {
        Name = name;
        Email = email;
        Password = password;
        PhoneNumber = phoneNumber;
        Role = role;
    }

    public UserName Name { get; private set; }
    public Email Email { get; private set; }
    public Password Password { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }

    public static User Create(UserName name, Email email, Password password, PhoneNumber phoneNumber, UserRole role,
        UserId? id = null)
    {
        var user = new User(id ?? UserId.Create(Guid.NewGuid()).Value, name, email, password, phoneNumber, role);
        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
        return user;
    }

    public void Update(UserName name, Email email, Password password, PhoneNumber phoneNumber, UserRole role)
    {
        Name = name;
        Email = email;
        Password = password;
        PhoneNumber = phoneNumber;
        Role = role;
        
        AddDomainEvent(new UserUpdatedDomainEvent(this));
    }

    public enum UserRole
    {
        Default = 1,
        Seller,
        Admin
    }
}