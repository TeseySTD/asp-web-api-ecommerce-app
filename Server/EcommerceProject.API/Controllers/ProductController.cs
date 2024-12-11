using EcommerceProject.API.Http.Product.Requests;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;
using EcommerceProject.Application.UseCases.Products.Commands.DeleteProduct;
using EcommerceProject.Application.UseCases.Products.Commands.UpdateProduct;
using EcommerceProject.Application.UseCases.Products.Queries.GetProductById;
using EcommerceProject.Application.UseCases.Products.Queries.GetProducts;
using EcommerceProject.Core.Common;
using EcommerceProject.Core.Models.Categories.ValueObjects;
using EcommerceProject.Core.Models.Products.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/products")]
public class ProductController : ApiController
{
    public ProductController(ISender sender) : base(sender){}
    
    [HttpGet]
    public async Task<ActionResult<GetProductsResponse>> GetProducts(CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery();
        var result = await Sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Errors);
        else
            return Ok(result.Value);
    }
    
    [HttpGet("{id:guid}")] 
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(ProductId.Create(id).Value);
        var result = await Sender.Send(query, cancellationToken);
        
        if(result.IsFailure)
            return NotFound(result.Errors);
        else
            return Ok(result.Value.Value);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> AddProduct([FromBody] AddProductRequest request, CancellationToken cancellationToken)
    {
        ProductDto dto = new ProductDto(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.Price,
            request.Quantity,
            null
        );
        var cmd = new CreateProductCommand(dto, CategoryId.Create(request.CategoryId).Value);
        var result = await Sender.Send(cmd, cancellationToken);

        return result.Map<ActionResult<Guid>>(
            onSuccess: () => Ok(dto.Id),
            onFailure: errors => BadRequest(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateProduct([FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        ProductDto dto = new ProductDto(
            Id: request.Id,
            Title: request.Title,
            Description: request.Description,
            Price: request.Price,
            Quantity: request.Quantity,
            Category: null
        );
        
        var cmd = new UpdateProductCommand(dto, CategoryId.Create(request.CategoryId).Value);
        var result = await Sender.Send(cmd, cancellationToken);
        
        if(result.IsFailure)
            return BadRequest(result.Errors);
        return Ok();
    }
    
    [HttpDelete("{id:guid}")] 
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var cmd = new DeleteProductCommand(ProductId.Create(id).Value);
        
        var result = await Sender.Send(cmd);
        
        if(result.IsFailure)
            return NotFound(result.Errors);
        else
            return Ok();
    }

}
