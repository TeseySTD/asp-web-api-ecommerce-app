using System;
using System.ComponentModel.DataAnnotations;

namespace EcommerceProject.Persistence.Entities;

public class ProductEntity
{
    [Key]
    public Guid Id { get; set;}
    [Required]
    public string Title { get; set; } = null!;
    [Required]
    [StringLength(100)]
    public string Description { get; set; } = null!;
    [Required]
    public decimal Price { get; set; }
}
