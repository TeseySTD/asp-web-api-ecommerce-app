using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ApiController
    {
        public UserController(ISender sender) : base(sender)
        {
        }
    }
}
