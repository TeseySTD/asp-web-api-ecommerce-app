using System;
using EcommerceProject.Core.Abstractions.Classes;
using EcommerceProject.Core.Models.Products;

namespace EcommerceProject.Core.Models.Orders.Entities;

public class OrderItem : Entity<Guid>
{
    private Product _product = null!;
    private int _quantity = 0;
    private decimal _price;

    private OrderItem(Guid id, Product product, int quantity, decimal price) : base(id)
    {
        Product = product;
        Quantity = quantity;
        Price = price;
    }

    public Product Product
    {
        get => _product;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Product));
            _product = value; 
        }
    }
    public int Quantity {
        get => _quantity;
        set
        {
            if (value < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(Quantity));
            _quantity = value;
        }
    }
    public decimal Price {
        get => _price;
        set
        {
            if (value < 0) throw new ArgumentException("Price cannot be negative", nameof(Price));
            _price = value;
        }
    }

    public static OrderItem Create(Product product, int quantity, decimal price) => new(new Guid(), product, quantity, price);
}
