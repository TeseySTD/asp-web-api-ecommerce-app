using EcommerceProject.Application.Common.Classes.Validation;
using EcommerceProject.Core.Common;
using MediatR;

namespace EcommerceProject.Application.Common.Interfaces.Messaging;

public interface ICommandBase;
public interface ICommand: IRequest<Result>, ICommandBase;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase
    where TResponse : notnull;