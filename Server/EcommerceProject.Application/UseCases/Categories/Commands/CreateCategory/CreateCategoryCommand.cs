using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.UseCases.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Description) : ICommand<CategoryId>;
