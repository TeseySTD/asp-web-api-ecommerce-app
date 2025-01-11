using MediatR;
using Shared.Core.Validation;

namespace Shared.Core.CQRS;

public interface IQueryHandler<in TQuery, TResponse> :
    IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
}