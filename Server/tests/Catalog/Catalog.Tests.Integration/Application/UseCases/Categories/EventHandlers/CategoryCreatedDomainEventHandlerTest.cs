using Catalog.Application.UseCases.Categories.EventHandlers;
using Catalog.Core.Models.Categories;
using Catalog.Core.Models.Categories.Events;
using Catalog.Core.Models.Categories.ValueObjects;
using Catalog.Tests.Integration.Common;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shared.Messaging.Events.Category;

namespace Catalog.Tests.Integration.Application.UseCases.Categories.EventHandlers;

public class CategoryCreatedDomainEventHandlerTest : IntegrationTest
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CategoryCreatedDomainEventHandler> _logger;

    public CategoryCreatedDomainEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<CategoryCreatedDomainEventHandler>>();
    }


    [Fact]
    public async Task WhenCategoryCreatedDomainEventRaised_ThenPublishesMessageWithCorrectData()
    {
        // Arrange 
        var categoryId = CategoryId.Create(Guid.NewGuid()).Value;
        var name = CategoryName.Create("Test category").Value;

        var domainEvent = new CategoryCreatedDomainEvent(categoryId, name);
        var handler = new CategoryCreatedDomainEventHandler(
            _logger,
            _publishEndpoint
        );

        // Act
        await handler.Handle(domainEvent, default);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<CategoryCreatedEvent>(
            e => e.CategoryId == categoryId.Value &&
                 e.CategoryName == name.Value)
        );
    }
}