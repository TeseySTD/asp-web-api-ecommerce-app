﻿using System.Security.Claims;
using EcommerceProject.API.Http;
using EcommerceProject.API.Http.Auth.Requests;
using EcommerceProject.API.Http.Auth.Responses;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Application.UseCases.Authentication.Commands.Login;
using EcommerceProject.Application.UseCases.Authentication.Commands.Logout;
using EcommerceProject.Application.UseCases.Authentication.Commands.RefreshToken;
using EcommerceProject.Application.UseCases.Authentication.Commands.Register;
using EcommerceProject.Core.Models.Users.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/auth")]
public class AuthenticationController : ApiController
{
    public AuthenticationController(ISender sender) : base(sender)
    {
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokensResponse>> Login(LoginUserRequest request)
    {
        var cmd = new LoginUserCommand(request.Email, request.Password);
        var result = await Sender.Send(cmd);

        return result.Map<ActionResult<TokensResponse>>(
            onSuccess: value => Ok(new TokensResponse(value.AccessToken, value.RefreshToken)),
            onFailure: errors => BadRequest(Envelope.Of(errors)));
    }

    [HttpPost("register")]
    public async Task<ActionResult<TokensResponse>> Register(RegisterUserRequest request)
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

        return result.Map<ActionResult<TokensResponse>>(
            onSuccess: value => Ok(new TokensResponse(value.AccessToken, value.RefreshToken)),
            onFailure: errors => BadRequest(Envelope.Of(errors)));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<TokensResponse>> Refresh(string refreshToken)
    {
        var cmd = new RefreshTokenCommand(refreshToken);
        var result = await Sender.Send(cmd);

        return result.Map<ActionResult<TokensResponse>>(
            onSuccess: value => Ok(new TokensResponse(value.AccessToken, value.RefreshToken)),
            onFailure: errors => BadRequest(Envelope.Of(errors)));
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task<IActionResult> Logout()
    {
        var token = HttpContext.User.FindFirstValue("userId");
        Guid id = Guid.TryParse(token, out Guid parse) ? parse : Guid.Empty;

        var cmd = new LogoutUserCommand(UserId.Create(id).Value);

        var result = await Sender.Send(cmd);

        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => BadRequest(Envelope.Of(errors)));
    }
}