using EcommerceProject.API.Http;
using EcommerceProject.API.Http.User.Requests;
using EcommerceProject.API.Http.User.Responses;
using EcommerceProject.Application.Dto.User;
using EcommerceProject.Application.UseCases.Users.Commands.CreateUser;
using EcommerceProject.Application.UseCases.Users.Commands.DeleteUser;
using EcommerceProject.Application.UseCases.Users.Commands.UpdateUser;
using EcommerceProject.Application.UseCases.Users.Queries.GetUserById;
using EcommerceProject.Application.UseCases.Users.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/users")]
public class UserController : ApiController
{
    public UserController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<ActionResult<GetUsersResponse>> GetUsers()
    {
        var query = new GetUsersQuery(); 
        var result = await Sender.Send(query);
        
        return result.Map<ActionResult<GetUsersResponse>>(
            onSuccess: value => Ok(new GetUsersResponse(value)),
            onFailure: errors => NotFound(Envelope.Of(errors)));
    }

    [HttpGet(template: "{id:guid}")]
    public async Task<ActionResult<UserReadDto>> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery(id);
        var result = await Sender.Send(query);

        return result.Map<ActionResult<UserReadDto>>(
            onSuccess: value => Ok(value),
            onFailure: errors => NotFound(Envelope.Of(errors)));
    }
    
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        var dto = new UserUpdateDto(
            Name: request.Name,
            Email: request.Email,
            Password: request.Password,
            PhoneNumber: request.PhoneNumber,
            Role: request.Role
        );
        
        var cmd = new UpdateUserCommand(id, dto);
        var result = await Sender.Send(cmd);

        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => NotFound(Envelope.Of(errors)));
    }
    
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUser(Guid id)
    {
        var cmd = new DeleteUserCommand(id);
        var result = await Sender.Send(cmd);

        return result.Map<ActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => NotFound(Envelope.Of(errors)));
    }
}

