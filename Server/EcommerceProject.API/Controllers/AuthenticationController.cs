using EcommerceProject.API.Http.Auth.Requests;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Application.UseCases.Authentication.Commands.Login;
using EcommerceProject.Application.UseCases.Authentication.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/auth")]
public class AuthenticationController : ApiController
{
    public AuthenticationController(ISender sender) : base(sender)
    {
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request)
    {
        var cmd = new LoginUserCommand(request.Email, request.Password);
        var result = await Sender.Send(cmd);
        
        return result.Map<IActionResult>(
            onSuccess: value => Ok(value),
            onFailure: errors => BadRequest(errors));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request)
    {
        var dto = new UserWriteDto(
            Name: request.Name,
            Email: request.Email,
            Password: request.Password,
            PhoneNumber: request.PhoneNumber,
            Role: request.Role
        );

        var cmd = new RegisterUserCommand(dto);
        var result = await Sender.Send(cmd);

        return result.Map<IActionResult>(
            onSuccess: value => Ok(value),
            onFailure: errors => BadRequest(errors));
    }
}