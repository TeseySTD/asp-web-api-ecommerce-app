using Shared.Core.CQRS;
using Users.Core.Models.Entities;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Authentication.Commands.EmailVerification;

public record EmailVerificationCommand(Guid Id) : ICommand;