using Microsoft.AspNetCore.Mvc;
using EcommerceProject.Core;
using EcommerceProject.Core.Models;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
using EcommerceProject.API.Contracts;
using EcommerceProject.Application.Dto;
using EcommerceProject.Application.Products.Commands.CreateProduct;
using EcommerceProject.Application.Products.Commands.DeleteProduct;
using EcommerceProject.Application.Products.Commands.UpdateProduct;
using EcommerceProject.Application.Products.Queries.GetProductById;
using EcommerceProject.Application.Products.Queries.GetProducts;
using EcommerceProject.Core.Models.Products;
using EcommerceProject.Core.Models.Products.ValueObjects;
using MediatR;

namespace EcommerceProject.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ISender _sender;
    
    public ProductController(ISender sender)
    {
        _sender = sender;
    }
    
    [HttpGet]
    public async Task<ActionResult<GetProductsResponse>> GetProducts(CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery();
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return NotFound(result.Error);
        else
            return Ok(result.Value);
    }
    
    [HttpGet("{id:guid}")] 
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(ProductId.Of(id));
        var result = await _sender.Send(query, cancellationToken);
        
        if(result.IsFailure)
            return NotFound(result.Error);
        else
            return Ok(result.Value);
    }
    
    [HttpPost]
    public async Task<ActionResult<Guid>> AddProduct([FromBody] AddProductRequest request, CancellationToken cancellationToken)
    {
        ProductDto dto = new ProductDto(
            Guid.NewGuid(),
            request.Title,
            request.Description,
            request.Price,
            request.Quantity
        );
        var cmd = new CreateProductCommand(dto);
        var result = await _sender.Send(cmd, cancellationToken);
        
        if (result.IsFailure)
            return NotFound(result.Error);
        else
            return Ok();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken)
    {
        var cmd = new UpdateProductCommand(productDto);
        var result = await _sender.Send(cmd, cancellationToken);
        
        if(result.IsFailure)
            return BadRequest(result.Error);
        return Ok();
    }
    
    [HttpDelete("{id:guid}")] 
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var cmd = new DeleteProductCommand(ProductId.Of(id));
        
        var result = await _sender.Send(cmd);
        
        if(result.IsFailure)
            return NotFound(result.Error);
        else
            return Ok();
    }

}
