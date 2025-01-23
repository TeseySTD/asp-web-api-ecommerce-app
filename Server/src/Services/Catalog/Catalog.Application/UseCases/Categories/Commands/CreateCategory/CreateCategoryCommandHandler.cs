using System.Text.Json;
using Catalog.Application.Common.Interfaces;
using Catalog.Application.Dto.Category;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Mapster;
using Microsoft.Extensions.Caching.Distributed;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Catalog.Application.UseCases.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CategoryId>
{
    private readonly IApplicationDbContext _context;
    private readonly IDistributedCache _cache;

    public CreateCategoryCommandHandler(IApplicationDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<Result<CategoryId>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var result = await CreateCategory(request, cancellationToken);

        if (result.IsSuccess)
            await _cache.SetStringAsync($"category-{result.Value.Id.Value}",
                JsonSerializer.Serialize(result.Value.Adapt<CategoryDto>()),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                });

        return result.Map(
            onSuccess: value => value.Id,
            onFailure: errors => Result<CategoryId>.Failure(errors));
    }

    private async Task<Result<Category>> CreateCategory(CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        var newCategory = Category.Create(
            name: CategoryName.Create(request.Name).Value,
            description: CategoryDescription.Create(request.Description).Value
        );

        try
        {
            await _context.Categories.AddAsync(newCategory, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return newCategory;
        }
        catch (Exception e)
        {
            return new Error(e.Message, e.StackTrace ?? string.Empty);
        }
    }
}