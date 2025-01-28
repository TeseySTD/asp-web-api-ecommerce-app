using Catalog.Application.Dto.Image;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Categories.Commands.AddCategoryImages;

public record AddCategoryImagesCommand(Guid CategoryId, IEnumerable<ImageDto> Images) : ICommand;