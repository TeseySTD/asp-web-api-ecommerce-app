using System;
using System.ComponentModel.DataAnnotations;
using EcommerceProject.Core.Models;

namespace EcommerceProject.Persistence.Entities;

public class ProductEntity
{
    [Key]
    public Guid Id { get; set;}
    [Required]
    [StringLength(maximumLength:Product.MAX_TITLE_LENGTH, MinimumLength = Product.MIN_TITLE_LENGTH)]
    public string Title { get; set; } = null!;
    [Required]
    [StringLength(maximumLength:Product.MAX_DESCRIPTION_LENGTH, MinimumLength = Product.MIN_DESCRIPTION_LENGTH)]
    public string Description { get; set; } = null!;
    [Required]
    [Range((double)Product.MIN_PRICE, (double)Product.MAX_PRICE)]
    public decimal Price { get; set; }
}
