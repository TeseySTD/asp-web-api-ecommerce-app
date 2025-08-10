using Basket.API.Data;
using Basket.API.Models.Cart;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;
using Weasel.Core;

namespace Basket.Tests.Integration.Common;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("catalog-api")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithName("catalog.test.database")
        .Build();

    private Respawner _respawner = null!;
    private NpgsqlConnection _connection = null!;
    private ServiceProvider? _serviceProvider;
    public string ConnectionString => _dbContainer.GetConnectionString();

    public ServiceProvider ServiceProvider => _serviceProvider ??
                                              throw new InvalidOperationException("ServiceProvider is not initialized. Call InitializeAsync first.");

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "ConnectionStrings:Database", ConnectionString }
            }!)
            .Build();

        services.AddDataLayerServices(config);
        _serviceProvider = services.BuildServiceProvider();

        // Get the document store and apply schema changes
        var documentStore = _serviceProvider.GetRequiredService<IDocumentStore>();
        await documentStore.Storage.ApplyAllConfiguredChangesToDatabaseAsync();

        // Ensure the ProductCart table exists
        await documentStore.Storage.Database.EnsureStorageExistsAsync(typeof(ProductCart));

        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_connection, new()
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }

    public async Task ResetAsync() => await _respawner.ResetAsync(_connection);

    public async Task DisposeAsync()
    {
        _serviceProvider?.Dispose();
        await _connection.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}