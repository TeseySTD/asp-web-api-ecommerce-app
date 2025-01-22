using Catalog.Core.Models.Categories.ValueObjects;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(string Name, string Description) : ICommand<CategoryId>;
