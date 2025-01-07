using EcommerceProject.Application.Common.Interfaces;
using EcommerceProject.Application.Common.Interfaces.Messaging;
using EcommerceProject.Application.UseCases.Users.Commands.CreateUser;
using EcommerceProject.Core.Common;
using MediatR;

namespace EcommerceProject.Application.UseCases.Authentication.Commands.Register;

public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, string>
{
    private readonly ISender _sender;
    private readonly IJwtTokenProvider _jwtTokenProvider;

    public RegisterUserCommandHandler(ISender sender, IJwtTokenProvider jwtTokenProvider)
    {
        _sender = sender;
        _jwtTokenProvider = jwtTokenProvider;
    }

    public async Task<Result<string>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var createUserCommand = new CreateUserCommand(request.Value);
        var userResult = await _sender.Send(createUserCommand);

        if (userResult.IsFailure)
            return Result<string>.Failure(userResult.Errors);
        
        var user = userResult.Value;

        return _jwtTokenProvider.GenerateToken(user);
    }
}