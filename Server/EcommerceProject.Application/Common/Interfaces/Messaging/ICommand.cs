using EcommerceProject.Application.Common.Classes.Validation;
using MediatR;

namespace EcommerceProject.Application.Common.Interfaces.Messaging;

public interface ICommand: IRequest<Result>;
public interface ICommand<TResponse>  : IRequest<Result<TResponse>>
    where TResponse : notnull;