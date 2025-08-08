using Catalog.Core.Models.Products.ValueObjects;
using MediatR;
using Shared.Core.Domain.Classes;
using Shared.Core.Domain.Interfaces;

namespace Catalog.Core.Models.Products.Events;

public record ProductCreatedDomainEvent(Product Product) : DomainEvent;