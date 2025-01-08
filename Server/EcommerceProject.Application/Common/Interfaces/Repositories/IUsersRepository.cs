using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.Entities;
using EcommerceProject.Core.Models.Users.ValueObjects;

namespace EcommerceProject.Application.Common.Interfaces.Repositories;

public interface IUsersRepository
{
    Task<IEnumerable<User>> Get(CancellationToken cancellationToken);
    Task<User?> FindById(UserId userId, CancellationToken cancellationToken);
    Task<User?> FindByEmail(Email email, CancellationToken cancellationToken);
    Task<Result> Add(User user, CancellationToken cancellationToken);
    Task<Result> Update(User user, CancellationToken cancellationToken);
    Task<Result> Delete(UserId user, CancellationToken cancellationToken);
    Task<bool> Exists(UserId user, CancellationToken cancellationToken);
    Task<bool> Exists(Email email, CancellationToken cancellationToken);
    Task<RefreshToken?> GetRefreshToken(string refreshToken, CancellationToken cancellationToken);
    Task<Result> AddRefreshToken(User user, RefreshToken refreshToken, CancellationToken cancellationToken);
    Task<Result> RemoveRefreshToken(string refreshToken, CancellationToken cancellationToken);
    Task<Result> RemoveUserRefreshTokens(UserId userId, CancellationToken cancellationToken);
 }