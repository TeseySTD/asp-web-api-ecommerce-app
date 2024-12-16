using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;

namespace EcommerceProject.Application.UseCases.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(CategoryDto Value) : ICommand;
