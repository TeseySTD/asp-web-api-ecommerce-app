using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.Common.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> Get(CancellationToken cancellationToken);
    Task<User?> FindById(UserId userId, CancellationToken cancellationToken);
    Task<Result> Add(User user, CancellationToken cancellationToken);
    Task<Result> Update(User user, CancellationToken cancellationToken);
    Task<Result> Delete(UserId user, CancellationToken cancellationToken);
    Task<bool> Exists(UserId user, CancellationToken cancellationToken);
}