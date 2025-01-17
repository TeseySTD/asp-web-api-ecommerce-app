using MediatR;
using Shared.Core.Validation;
using Shared.Core.Validation.Result;

namespace Shared.Core.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull
{
}