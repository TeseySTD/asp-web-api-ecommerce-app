using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Common;
using MediatR;

namespace EcommerceProject.Application.Common.Interfaces.Messaging;

public interface ICommandHandler<in TCommand> 
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

public interface ICommandHandler<in TCommand, TResponse> 
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
}