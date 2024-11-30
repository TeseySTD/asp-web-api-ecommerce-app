﻿using EcommerceProject.Application.Common.Classes.Validation;
using MediatR;

namespace EcommerceProject.Application.Common.Interfaces.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
    where TResponse : notnull
{
}