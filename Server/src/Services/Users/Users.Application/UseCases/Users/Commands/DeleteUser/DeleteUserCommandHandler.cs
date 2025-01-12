﻿using Microsoft.EntityFrameworkCore;
using Shared.Core.CQRS;
using Shared.Core.Validation;
using Users.Application.Common.Interfaces;
using Users.Core.Models.ValueObjects;

namespace Users.Application.UseCases.Users.Commands.DeleteUser;

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteUserCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var result = Result.TryFail()
            .CheckError(!await _context.Users.AnyAsync(u => u.Id == UserId.Create(request.Id).Value, cancellationToken),
                new Error("User not exists.", $"User with id: {request.Id} not exists."))
            .Build();
        if (result.IsFailure)
            return result;

        await _context.Users.Where(u => u.Id == UserId.Create(request.Id).Value).ExecuteDeleteAsync(cancellationToken);

        return Result.Success();
    }
}