using MediatR;
using Shared.Core.Validation;

namespace Shared.Core.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull
{
}