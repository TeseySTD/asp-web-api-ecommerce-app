using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.UseCases.Categories.Commands.CreateCategory;

public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand>
{
    private readonly ICategoriesRepository _categoriesRepository;

    public CreateCategoryCommandHandler(ICategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Result> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var newCategory = Category.Create(
            name: CategoryName.Create(request.Name).Value,
            description: CategoryDescription.Create(request.Description).Value
        );

        return await _categoriesRepository.Add(newCategory, cancellationToken);
    }
}