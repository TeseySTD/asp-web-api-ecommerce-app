using Microsoft.EntityFrameworkCore;
using Users.Application.Common.Interfaces;
using Users.Persistence;

namespace Users.Tests.Integration.Common;

public class IntegrationTest : BaseIntegrationTest, IAsyncLifetime
{
    private DatabaseFixture _databaseFixture;
    protected readonly IApplicationDbContext ApplicationDbContext; 
    protected IntegrationTest(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(databaseFixture.ConnectionString)
            .Options;

        var dbContext = new ApplicationDbContext(options);

        ApplicationDbContext = dbContext;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _databaseFixture.ResetAsync();
}