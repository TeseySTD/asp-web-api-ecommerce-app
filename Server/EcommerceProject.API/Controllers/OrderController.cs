using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers
{
    [Route("api/orders")]
    public class OrderController : ApiController
    {
        public OrderController(ISender sender) : base(sender)
        {
        }
    }
}
