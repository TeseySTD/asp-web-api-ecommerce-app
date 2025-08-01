using Basket.API.Data.Abstractions;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Basket.Tests.Integration.Common;

public class IntegrationTest : BaseIntegrationTest, IAsyncLifetime
{
    private DatabaseFixture _databaseFixture;
    public ICartRepository CartRepository { get; private set; }
    public IDocumentSession Session { get; private set; }
    
    protected IntegrationTest(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        CartRepository = _databaseFixture.ServiceProvider.GetRequiredService<ICartRepository>();
        Session = _databaseFixture.ServiceProvider.GetRequiredService<IDocumentSession>();
    }

    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual Task DisposeAsync() => _databaseFixture.ResetAsync();
}