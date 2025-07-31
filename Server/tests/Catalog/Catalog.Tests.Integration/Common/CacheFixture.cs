using StackExchange.Redis;
using Testcontainers.Redis;

namespace Catalog.Tests.Integration.Common;

public class CacheFixture : IAsyncLifetime
{
    private readonly RedisContainer _cacheContainer = new RedisBuilder()
        .WithName("catalog.test.cache")
        .WithImage("redis")
        .Build();

    private IConnectionMultiplexer? _connection;
    private IDatabase? _database;

    public string ConnectionString => _cacheContainer.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _cacheContainer.StartAsync();
        var options = ConfigurationOptions.Parse(ConnectionString);
        options.AllowAdmin = true;
        
        _connection = ConnectionMultiplexer.Connect(options);
        _database = _connection.GetDatabase();
    }

    public async Task ResetAsync()
    {
        if (_database != null)
        {
            var server = _connection!.GetServer(_cacheContainer.Hostname, _cacheContainer.GetMappedPublicPort(6379));
            await server.FlushDatabaseAsync();
        }
    }

    public async Task DisposeAsync()
    {
        _connection?.Dispose();
        await _cacheContainer.StopAsync();
    }
}