using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Categories.Events;

public record CategoryUpdatedDomainEvent(Category Category) : IDomainEvent;