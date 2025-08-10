using Testcontainers.RabbitMq;

namespace Catalog.Tests.Integration.Common;

public class MessageBrokerFixture : IAsyncLifetime
{
    private RabbitMqContainer _messageBrokerContainer = new RabbitMqBuilder()
        .WithImage("rabbitmq:management")
        .WithName("catalog.test.messagebroker")
        .WithHostname("ecommerce-mq")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build(); 
    
    public string ConnectionString => _messageBrokerContainer.GetConnectionString();
    public string UserName => "guest";
    public string Password => "guest";


    public Task InitializeAsync()
    {
        return _messageBrokerContainer.StartAsync();
    }

    public Task DisposeAsync()
    {
        return _messageBrokerContainer.StopAsync();
    }
}
