using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Core.Common;

namespace EcommerceProject.Application.UseCases.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly ICategoriesRepository _categoriesRepository;

    public GetCategoryByIdQueryHandler(ICategoriesRepository categoriesRepository)
    {
        _categoriesRepository = categoriesRepository;
    }

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _categoriesRepository.FindById(request.Id, cancellationToken);
        if (result != null)
        {
            var category = new CategoryDto
            (
                Id: result.Id.Value,
                Name: result.Name.Value,
                Description: result.Description.Value
            );
            return category;
        }
        else
            return Error.NotFound;
    }
}