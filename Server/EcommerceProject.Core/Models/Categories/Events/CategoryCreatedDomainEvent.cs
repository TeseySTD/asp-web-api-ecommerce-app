using EcommerceProject.Core.Common.Abstractions.Interfaces;
using EcommerceProject.Core.Models.Categories.ValueObjects;

namespace EcommerceProject.Core.Models.Categories.Events;

public record CategoryCreatedDomainEvent(CategoryId CategoryId) : IDomainEvent;