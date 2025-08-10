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

public class CategoryUpdatedDomainEventHandlerTest : IntegrationTest
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CategoryUpdatedDomainEventHandler> _logger;

    public CategoryUpdatedDomainEventHandlerTest(DatabaseFixture databaseFixture) : base(databaseFixture)
    {
        _publishEndpoint = Substitute.For<IPublishEndpoint>();
        _logger = Substitute.For<ILogger<CategoryUpdatedDomainEventHandler>>();
    }

    [Fact]
    public async Task Handle_CategoryCreatedDomainEventRaised_ShouldPublisheMessageWithCorrectData()
    {
        // Arrange 
        var categoryId = CategoryId.Create(Guid.NewGuid()).Value;
        var name = CategoryName.Create("Test category").Value;
        var category = Category.Create(
            categoryId,
            name,
            CategoryDescription.Create("Test description").Value
        );
        
        var domainEvent = new CategoryUpdatedDomainEvent(category);
        var handler = new CategoryUpdatedDomainEventHandler(
            _logger,
            _publishEndpoint
        );

        // Act
        await handler.Handle(domainEvent, default);

        // Assert
        await _publishEndpoint.Received(1).Publish(Arg.Is<CategoryUpdatedEvent>(
            e => e.CategoryId == categoryId.Value &&
                 e.CategoryName == name.Value)
        );
    }
}