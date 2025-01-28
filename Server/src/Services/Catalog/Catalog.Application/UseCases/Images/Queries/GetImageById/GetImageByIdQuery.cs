using Catalog.Core.Models.Images;
using Shared.Core.CQRS;

namespace Catalog.Application.UseCases.Images.Queries.GetImageById;

public record GetImageByIdQuery(Guid Id) : IQuery<Image>;