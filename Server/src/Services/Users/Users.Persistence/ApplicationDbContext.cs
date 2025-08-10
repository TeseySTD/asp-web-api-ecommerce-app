using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Domain.Classes;
using Users.Application.Common.Interfaces;
using Users.Application.UseCases.Authentication.Commands.EmailVerification;
using Users.Core.Models;
using Users.Core.Models.Entities;

namespace Users.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}