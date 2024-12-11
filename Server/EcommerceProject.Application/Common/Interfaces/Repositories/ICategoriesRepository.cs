using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Application.Common.Interfaces.Repositories;

public interface ICategoriesRepository
{
    public Task<IEnumerable<Category>> Get(CancellationToken cancellationToken);
    public Task<Category?> FindById(CategoryId id, CancellationToken cancellationToken);
    public Task<Result> Add(Category category, CancellationToken cancellationToken);  
    public Task<Result> Update(CategoryId id, Category category, CancellationToken cancellationToken);
    public Task<Result> Delete(CategoryId id, CancellationToken cancellationToken);
    public Task<bool> Exists(CategoryId id, CancellationToken cancellationToken);
}