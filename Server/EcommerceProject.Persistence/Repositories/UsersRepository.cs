using EcommerceProject.Application.Common.Interfaces.Repositories;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Users;
using EcommerceProject.Core.Models.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EcommerceProject.Persistence.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly StoreDbContext _context;

    public UsersRepository(StoreDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> Get(CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<User?> FindById(UserId userId, CancellationToken cancellationToken)
    {
        var user = _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user;
    }

    public async Task<Result> Add(User user, CancellationToken cancellationToken)
    {
        var result = Result.TryFail()
            .CheckError(await _context.Users.AnyAsync(u => u.Id == user.Id, cancellationToken),
                new Error("User already exists.", $"User with id: {user.Id} already exists."))
            .Build();

        if (result.IsSuccess)
        {
            await _context.Users.AddAsync(user, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return result;
    }

    public async Task<Result> Update(User user, CancellationToken cancellationToken)
    {
        var result = Result.TryFail()
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == user.Id, cancellationToken),
                new Error("User not exists.", $"User with id: {user.Id} not exists."))
            .Build();
        if (result.IsFailure)
            return result;

        await _context.Users
            .Where(u => u.Id == user.Id)
            .ExecuteUpdateAsync(u => u
                .SetProperty(pr => pr.Name, user.Name)
                .SetProperty(pr => pr.Email, user.Email)
                .SetProperty(pr => pr.Password, user.Password)
                .SetProperty(pr => pr.PhoneNumber, user.PhoneNumber)
                .SetProperty(pr => pr.Role, user.Role)
            , cancellationToken);

        return Result.Success();
    }

    public async Task<Result> Delete(UserId id, CancellationToken cancellationToken)
    {
        var result = Result.TryFail()
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == id, cancellationToken),
                new Error("User not exists.", $"User with id: {id} not exists."))
            .Build();
        if (result.IsFailure)
            return result;

        await _context.Users.Where(u => u.Id == id).ExecuteDeleteAsync(cancellationToken);
        
        return Result.Success();
    }

    public async Task<bool> Exists(UserId id, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(u => u.Id == id, cancellationToken);
    }
}