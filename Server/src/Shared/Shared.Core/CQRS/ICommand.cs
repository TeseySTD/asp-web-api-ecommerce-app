using MediatR;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Shared.Core.CQRS;

public interface ICommandBase;
public interface ICommand: IRequest<Result>, ICommandBase;
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, ICommandBase
    where TResponse : notnull;