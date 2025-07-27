using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Persistence;

namespace Ordering.Tests.Integration.Common;

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

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => _databaseFixture.ResetAsync();
}