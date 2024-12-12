using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Dto.Product;

namespace EcommerceProject.Application.UseCases.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Description) : ICommand<Guid>;
