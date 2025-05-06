using Microsoft.EntityFrameworkCore;
using Users.Application.UseCases.Authentication.Commands.EmailVerification;
using Users.Core.Models;
using Users.Core.Models.Entities;

namespace Users.Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}