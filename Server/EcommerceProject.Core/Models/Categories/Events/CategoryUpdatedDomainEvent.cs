using EcommerceProject.Core.Common.Abstractions.Interfaces;

namespace EcommerceProject.Core.Models.Categories.Events;

public record CategoryUpdatedDomainEvent(Category Category) : IDomainEvent;