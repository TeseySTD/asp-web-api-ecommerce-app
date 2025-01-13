using Catalog.Core.Models.Categories.ValueObjects;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Commands.DeleteCategory;

public record DeleteCategoryCommand(CategoryId Id) : ICommand;