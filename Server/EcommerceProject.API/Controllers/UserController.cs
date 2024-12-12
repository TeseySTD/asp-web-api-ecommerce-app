using EcommerceProject.Core.Models.Users;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/users")]
public class UserController : ApiController
{
    public UserController(ISender sender) : base(sender)
    {
    }

    // [HttpGet]
    // public async Task<IActionResult> GetUsers()
    // {
    //     
    // }
    //
    // [HttpPost]
    // public async Task<ActionResult<Guid>> AddUser(User user)
    // {
    //     
    // }
}

