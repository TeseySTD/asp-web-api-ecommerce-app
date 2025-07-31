using Catalog.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using Testcontainers.PostgreSql;

namespace Catalog.Tests.Integration.Common;

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

    public string ConnectionString => _dbContainer.GetConnectionString();
    
    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
        
        _connection = new NpgsqlConnection(ConnectionString);
        await _connection.OpenAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(_dbContainer.GetConnectionString())
            .Options;
        
        using var migrationContext = new ApplicationDbContext(options);
        migrationContext.Database.Migrate();
        
        _respawner = await Respawner.CreateAsync(_connection , new()
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"]
        });
    }
    
    public async Task ResetAsync() => await _respawner.ResetAsync(_connection);

    public async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await _dbContainer.StopAsync();
    }
}