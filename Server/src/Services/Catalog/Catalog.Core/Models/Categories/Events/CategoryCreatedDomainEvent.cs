using Catalog.Core.Models.Categories.ValueObjects;
using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Categories.Events;

public record CategoryCreatedDomainEvent(CategoryId CategoryId, CategoryName CategoryName) : DomainEvent;