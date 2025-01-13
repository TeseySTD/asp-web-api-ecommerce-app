using Catalog.Application.Dto.Category;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Commands.UpdateCategory;

public record UpdateCategoryCommand(CategoryDto Value) : ICommand;
