using Ordering.Core.Models.Customers.ValueObjects;
using Shared.Core.Domain.Classes;

namespace Ordering.Core.Models.Customers;

public class Customer : AggregateRoot<CustomerId>
{
    private Customer() : base(default!)
    {
    }

    private Customer(CustomerId id, CustomerName name, Email email, PhoneNumber phoneNumber) : base(id)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public CustomerName Name { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }

    public static Customer Create(CustomerName name, Email email, PhoneNumber phoneNumber,
        CustomerId? id = null)
    {
        var user = new Customer(id ?? CustomerId.Create(Guid.NewGuid()).Value, name, email, phoneNumber);
        return user;
    }

    public void Update(CustomerName name, Email email, PhoneNumber phoneNumber)
    {
        Name = name;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}