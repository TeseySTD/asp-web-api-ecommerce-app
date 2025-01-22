using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Categories.Events;

public record CategoryUpdatedDomainEvent(Category Category) : DomainEvent;