using Catalog.Core.Models.Categories.ValueObjects;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Categories.Events;

public record CategoryCreatedDomainEvent(CategoryId CategoryId) : IDomainEvent;