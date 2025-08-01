namespace Basket.Tests.Integration.Common;

[CollectionDefinition(nameof(IntegrationTestsCollection))]
public class IntegrationTestsCollection :
    ICollectionFixture<DatabaseFixture>,
    ICollectionFixture<MessageBrokerFixture>
{
}