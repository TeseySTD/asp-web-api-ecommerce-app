using EcommerceProject.Core.Common.Abstractions.Interfaces;
using MediatR;

namespace EcommerceProject.Core.Models.Users.Events;

public record UserUpdatedDomainEvent(User User) : IDomainEvent;