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
    
    public Task<User?> FindByEmail(Email email, CancellationToken cancellationToken)
    {
        var user = _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user;
    }

    public async Task<Result> Add(User user, CancellationToken cancellationToken)
    {
        var result = Result.TryFail()
            .CheckError(await _context.Users.AnyAsync(u => u.Id == user.Id, cancellationToken),
                new Error("User already exists.", $"User with id: {user.Id} already exists."))
            .CheckError(await _context.Users.AnyAsync(u => u.Email == user.Email, cancellationToken),
                new Error("User already exists.", $"User with email: {user.Email} already exists."))
            .CheckError(await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber, cancellationToken),
                new Error("User already exists.", $"User with number: {user.PhoneNumber} already exists."))
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
            .DropIfFailed()
            .CheckError(await _context.Users.AnyAsync(u => u.Email == user.Email && u.Id != user.Id, cancellationToken),
                new Error("Incorrect email.", $"User with email: {user.Email} already exists."))
            .CheckError(await _context.Users.AnyAsync(u => u.PhoneNumber == user.PhoneNumber && u.Id != user.Id, cancellationToken),
                new Error("Incorrect phone number.", $"User with number: {user.PhoneNumber} already exists."))
            .Build();
        
        if (result.IsFailure)
            return result;
        
        var userToUpdate = await _context.Users.FindAsync([user.Id], cancellationToken);
        userToUpdate!.Update(
            name: user.Name,
            email: user.Email,
            phoneNumber: user.PhoneNumber,
            password: user.Password,
            role: user.Role
        );
        
        await _context.SaveChangesAsync(cancellationToken);
        
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
    
    public async Task<bool> Exists(Email email, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> CheckPassword(Email email, Password password, CancellationToken cancellationToken)
    {
        return await _context.Users.AnyAsync(u => u.Email == email && u.Password == password, cancellationToken);
    }
}