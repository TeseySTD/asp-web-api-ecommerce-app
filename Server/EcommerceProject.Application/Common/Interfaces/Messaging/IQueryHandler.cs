using EcommerceProject.Application.Common.Classes.Validation;
using MediatR;

namespace EcommerceProject.Application.Common.Interfaces.Messaging;

public interface IQueryHandler<in TQuery, TResponse> :
    IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
    where TResponse : notnull
{
}