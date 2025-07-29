namespace Catalog.Tests.Integration.Common;

[CollectionDefinition(nameof(IntegrationTestsCollection))]
public class IntegrationTestsCollection :
    ICollectionFixture<DatabaseFixture>
{
}