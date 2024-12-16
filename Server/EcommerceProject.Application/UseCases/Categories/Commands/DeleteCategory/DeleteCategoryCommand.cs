using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.UseCases.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(CategoryId Id) : ICommand;