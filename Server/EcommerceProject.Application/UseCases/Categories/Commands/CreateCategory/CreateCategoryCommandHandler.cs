using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EcommerceProject.Application.UseCases.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoriesRepository _categoriesRepository;

    public CreateCategoryCommandHandler(ICategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var newCategory = Category.Create(
            name: CategoryName.Create(request.Name).Value,
            description: CategoryDescription.Create(request.Description).Value
        );

        var result = await _categoriesRepository.Add(newCategory, cancellationToken);
        
        return result.Map<Result<Guid>>(
            onSuccess: () => Result<Guid>.Success(newCategory.Id.Value),
            onFailure: errors => Result<Guid>.Failure(errors));
    }
}