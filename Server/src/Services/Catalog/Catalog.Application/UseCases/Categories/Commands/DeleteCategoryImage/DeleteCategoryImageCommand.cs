using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Commands.DeleteCategoryImage;

public record DeleteCategoryImageCommand(Guid CategoryId, Guid ImageId) : ICommand;