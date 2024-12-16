using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.UseCases.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand>
{
    private readonly ICategoriesRepository _categoriesRepository;

    public UpdateCategoryCommandHandler(ICategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var updatedCategoryId = CategoryId.Create(request.Value.Id).Value;
        var updatedCategory = Category.Create(
            id: updatedCategoryId,
            name: CategoryName.Create(request.Value.Name).Value,
            description: CategoryDescription.Create(request.Value.Description).Value
        );
        
        return _categoriesRepository.Update(updatedCategoryId, updatedCategory, cancellationToken);
    }
}