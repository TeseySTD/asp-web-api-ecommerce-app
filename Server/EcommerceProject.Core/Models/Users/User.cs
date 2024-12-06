using EcommerceProject.Core.Common.Abstractions.Classes;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Core.Models.Users;

public class User : AggregateRoot<UserId>
{
    private User():base(default!){}
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
        UserId? id = null) =>
        new User(id ?? UserId.Create(Guid.NewGuid()), name, email, password, phoneNumber, role);

    public enum UserRole
    {
        Default = 1,
        Admin,
        Seller,
    }
}