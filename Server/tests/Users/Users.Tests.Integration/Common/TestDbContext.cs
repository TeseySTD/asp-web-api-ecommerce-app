using Microsoft.EntityFrameworkCore;
using Shared.Core.Domain.Classes;
using Users.Application.Common.Interfaces;
using Users.Core.Models;
using Users.Core.Models.Entities;
using Users.Persistence;

namespace Users.Tests.Integration.Common;

public class TestDbContext : DbContext, IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
    public new Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}