﻿using Carter;
using MediatR;
using Shared.Core.API;
using Users.API.Http.User.Requests;
using Users.API.Http.User.Responses;
using Users.Application.Dto.User;
using Users.Application.UseCases.Users.Commands.DeleteUser;
using Users.Application.UseCases.Users.Commands.UpdateUser;
using Users.Application.UseCases.Users.Queries.GetUserById;
using Users.Application.UseCases.Users.Queries.GetUsers;

namespace Users.API.Endpoints;

public class UsersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var routeGroup = app.MapGroup("/api/users");

        // Get all users
        routeGroup.MapGet("/", async (ISender sender) =>
        {
            var query = new GetUsersQuery();
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(new GetUsersResponse(value)),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        // Get user by ID
        routeGroup.MapGet("/{id:guid}", async (ISender sender, Guid id) =>
        {
            var query = new GetUserByIdQuery(id);
            var result = await sender.Send(query);

            return result.Map(
                onSuccess: value => Results.Ok(value),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        // Update user
        routeGroup.MapPut("/{id:guid}", async (ISender sender, Guid id, UpdateUserRequest request) =>
        {
            var dto = new UserUpdateDto(
                Name: request.Name,
                Email: request.Email,
                Password: request.Password,
                PhoneNumber: request.PhoneNumber,
                Role: request.Role
            );

            var cmd = new UpdateUserCommand(id, dto);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });

        // Delete user
        routeGroup.MapDelete("/{id:guid}", async (ISender sender, Guid id) =>
        {
            var cmd = new DeleteUserCommand(id);
            var result = await sender.Send(cmd);

            return result.Map(
                onSuccess: () => Results.Ok(),
                onFailure: errors => Results.NotFound(Envelope.Of(errors))
            );
        });
    }
}