using EcommerceProject.API.Http.Category.Requests;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Application.UseCases.Categories.Commands.CreateCategory;
using EcommerceProject.Application.UseCases.Categories.Commands.DeleteCategory;
using EcommerceProject.Application.UseCases.Categories.Commands.UpdateCategory;
using EcommerceProject.Application.UseCases.Categories.Queries.GetCategories;
using EcommerceProject.Application.UseCases.Categories.Queries.GetCategoryById;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/categories")]
public class CategoryController : ApiController
{
    public CategoryController(ISender sender):base(sender){}

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        var query = new GetCategoriesQuery();
        var result = await Sender.Send(query);

        if (result.IsFailure)
            return NotFound(result.Errors);
        return Ok(result.Value);
    }


    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetCategory(Guid id)
    {
        var query = new GetCategoryByIdQuery(CategoryId.Create(id).Value);

        var result = await Sender.Send(query);

        if (result.IsFailure)
            return NotFound(result.Errors);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> AddCategory([FromBody] AddCategoryRequest request)
    {
        var cmd = new CreateCategoryCommand(
            request.Name, request.Description
        );

        var result = await Sender.Send(cmd);

        if (result.IsFailure)
            return BadRequest(result.Errors);
        else
            return Ok();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCategory(CategoryDto request, CancellationToken cancellationToken)
    {
        var cmd = new UpdateCategoryCommand(request);
        var result = await Sender.Send(cmd, cancellationToken);

        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => BadRequest(errors)
        );
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken)
    {
        var cmd = new DeleteCategoryCommand(CategoryId.Create(id).Value);
        
        var result = await Sender.Send(cmd, cancellationToken);

        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => BadRequest(errors)
        );

    }
        

}