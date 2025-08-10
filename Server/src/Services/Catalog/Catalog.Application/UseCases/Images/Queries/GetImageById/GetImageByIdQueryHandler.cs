using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Images;
using Catalog.Core.Models.Images.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Images.Queries.GetImageById;

public class GetImageByIdQueryHandler : IQueryHandler<GetImageByIdQuery, Image>
{
    private readonly IApplicationDbContext _context;

    public GetImageByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Image>> Handle(GetImageByIdQuery request, CancellationToken cancellationToken)
    {
        var imageId = ImageId.Create(request.Id).Value;
        
        var image = await _context.Images.FindAsync(imageId);
        if (image == null)
            return new ImageNotFoundError(imageId.Value);
        else
            return image;
    }

    public sealed record ImageNotFoundError(Guid Id) : Error("Image not found", $"Image with id:{Id} not found");
}