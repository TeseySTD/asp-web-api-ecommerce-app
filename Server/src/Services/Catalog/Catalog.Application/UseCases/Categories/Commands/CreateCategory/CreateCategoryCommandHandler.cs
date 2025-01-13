using Catalog.Application.Common.Interfaces;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.ValueObjects;
using Shared.Core.CQRS;
using Shared.Core.Validation;

namespace Catalog.Application.UseCases.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CategoryId>
{
    private readonly IApplicationDbContext _context;

    public CreateCategoryCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CategoryId>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var newCategory = Category.Create(
            name: CategoryName.Create(request.Name).Value,
            description: CategoryDescription.Create(request.Description).Value
        );

        try
        {
            await _context.Categories.AddAsync(newCategory, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return newCategory.Id;
        }
        catch (Exception e)
        {
            return new Error(e.Message, e.StackTrace ?? string.Empty);
        }

    }
}