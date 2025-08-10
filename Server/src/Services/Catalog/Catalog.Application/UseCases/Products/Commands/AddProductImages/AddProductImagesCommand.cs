using Catalog.Application.Dto.Image;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Products.Commands.AddProductImages;

public record AddProductImagesCommand(Guid ProductId, Guid SellerId, IEnumerable<ImageDto> Images) : ICommand;