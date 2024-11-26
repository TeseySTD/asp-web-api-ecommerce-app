using System;
using System.Text.RegularExpressions;
using EcommerceProject.Core.Abstractions.Classes;

namespace EcommerceProject.Core.Models;

public class User : AggregateRoot<Guid>
{
    public const int MaxNameLength = 100;
    public const int MinNameLength = 1;
    public const string RegexEmail = @"^((?!\.)[\w\-_.]*[^.])(@\w+)(\.\w+(\.\w+)?[^.\W])$";
    public const int MinPasswordLength = 6;
    public const string RegexPhoneNumber = @"^((?:\+?3)?8)?[\s\-\(]*?(0\d{2})[\s\-\)]*?(\d{3})[\s\-]*?(\d{2})[\s\-]*?(\d{2})$";
    private string _name = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _phoneNumber = string.Empty;
    private UserRole _role = UserRole.Default;

    private User(Guid id, string name, string email, string password, string phoneNumber) : base(id)
    {
        Name = name;
        Email = email;
        Password = password;
        PhoneNumber = phoneNumber;
    }
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(Name));
            if (_name.Length > MaxNameLength || _name.Length < MinNameLength) throw new ArgumentException($"Name length must be between {MinNameLength} and {MaxNameLength}", nameof(Name));
            _name = value;
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(Email));
            if (!Regex.IsMatch(value, RegexEmail)) throw new ArgumentException("Invalid email address", nameof(Email));
            _email = value;
        }
    }
    public string Password
    {
        get => _password;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(Password));
            if (value.Length < MinPasswordLength) throw new ArgumentException($"Password length must be at least {MinPasswordLength}", nameof(Password));
            _password = value;
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        set
        {
            if (string.IsNullOrWhiteSpace(value)) throw new ArgumentNullException(nameof(PhoneNumber));
            if (!Regex.IsMatch(value, RegexPhoneNumber)) throw new ArgumentException("Invalid phone number", nameof(PhoneNumber));
            _phoneNumber = value;
        }
    }

    public UserRole Role
    {
        get => _role;
        set
        {
            if(Enum.IsDefined(typeof(UserRole), value)) 
                _role = value;
            else 
                throw new ArgumentException("Invalid user role", nameof(Role));
        }
    }

    public static User Create(string name, string email, string password, string phoneNumber, Guid id = new Guid()) =>
        new User(id, name, email, password, phoneNumber);

    public enum UserRole
    {
        Default = 1,
        Admin,
        Seller,
    }
}
