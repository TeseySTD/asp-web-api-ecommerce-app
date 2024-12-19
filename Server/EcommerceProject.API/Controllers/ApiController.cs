using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;


[ApiController]
public abstract class ApiController : ControllerBase
{
    private readonly ISender _sender;

    protected ApiController(ISender sender)
    {
        _sender = sender;
    }

    public ISender Sender => _sender;
}