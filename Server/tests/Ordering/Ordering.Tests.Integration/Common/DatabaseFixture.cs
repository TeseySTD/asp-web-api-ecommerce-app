using Microsoft.EntityFrameworkCore;
using Npgsql;
using Ordering.Persistence;
using Respawn;
using Testcontainers.PostgreSql;

namespace Ordering.Tests.Integration.Common;

public class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("ordering-api")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .WithName("ordering.test.database")
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