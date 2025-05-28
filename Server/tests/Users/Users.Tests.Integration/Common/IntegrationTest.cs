using Microsoft.EntityFrameworkCore;
using Users.Application.Common.Interfaces;
using Users.Persistence;

namespace Users.Tests.Integration.Common;

[Collection(nameof(IntegrationTestsCollection))]
public class IntegrationTest
{
    protected readonly IApplicationDbContext ApplicationDbContext; 
    protected IntegrationTest(DatabaseFixture databaseFixture)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseNpgsql(databaseFixture.ConnectionString, b => 
                b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
            .Options;

        var dbContext = new TestDbContext(options);

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        dbContext.Database.Migrate();
        
        ApplicationDbContext = dbContext;
    }
}