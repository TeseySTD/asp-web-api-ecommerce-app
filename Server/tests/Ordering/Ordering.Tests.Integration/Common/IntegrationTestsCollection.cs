namespace Ordering.Tests.Integration.Common;

[CollectionDefinition(nameof(IntegrationTestsCollection))]
public class IntegrationTestsCollection :
    ICollectionFixture<DatabaseFixture>

{
}