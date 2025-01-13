using System.Security.Claims;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Shared.Core.API;
using Users.API.Http.Auth.Requests;
using Users.API.Http.Auth.Responses;
using Users.Application.Dto.User;
using Users.Application.UseCases.Authentication.Commands.Login;
using Users.Application.UseCases.Authentication.Commands.Logout;
using Users.Application.UseCases.Authentication.Commands.RefreshToken;
using Users.Application.UseCases.Authentication.Commands.Register;
using Users.Core.Models.ValueObjects;

namespace Users.API.Endpoints;

public class AuthenticationModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroup = app.MapGroup("/api/auth");

        // Login
        routeGroup.MapPost("/login", async (ISender sender, LoginUserRequest request) =>
        {
            var cmd = new LoginUserCommand(request.Email, request.Password);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: value => Results.Ok(new TokensResponse(value.AccessToken, value.RefreshToken)),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });

        // Register
        routeGroup.MapPost("/register", async (ISender sender, RegisterUserRequest request) =>
        {
            var dto = new UserWriteDto(
                Name: request.Name,
                Email: request.Email,
                Password: request.Password,
                PhoneNumber: request.PhoneNumber,
                Role: request.Role
            );

            var cmd = new RegisterUserCommand(dto);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: value => Results.Ok(new TokensResponse(value.AccessToken, value.RefreshToken)),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });

        // Refresh token
        routeGroup.MapPost("/refresh", async (ISender sender, string refreshToken) =>
        {
            var cmd = new RefreshTokenCommand(refreshToken);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: value => Results.Ok(new TokensResponse(value.AccessToken, value.RefreshToken)),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        });

        // Logout
        routeGroup.MapDelete("/logout", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userIdClaim = user.FindFirstValue("userId");
            Guid id = Guid.TryParse(userIdClaim, out Guid parsed) ? parsed : Guid.Empty;

            var cmd = new LogoutUserCommand(id);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.BadRequest(Envelope.Of(errors))
            );
        })
        .RequireAuthorization();
    }
}