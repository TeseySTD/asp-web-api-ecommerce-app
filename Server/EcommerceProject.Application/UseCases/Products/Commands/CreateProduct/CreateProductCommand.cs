using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(ProductDto Value, CategoryId CategoryId) : ICommand;


