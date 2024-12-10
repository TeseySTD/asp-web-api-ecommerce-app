using EcommerceProject.API.Http.Product.Requests;
using Microsoft.AspNetCore.Mvc;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Application.Products.Commands.CreateProduct;
using EcommerceProject.Application.Products.Commands.DeleteProduct;
using EcommerceProject.Application.Products.Commands.UpdateProduct;
using EcommerceProject.Application.Products.Queries.GetProductById;
using EcommerceProject.Application.Products.Queries.GetProducts;
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
            return NotFound(result.Errors);
        else
            return Ok(result.Value);
    }
    
    [HttpGet("{id:guid}")] 
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(ProductId.Create(id));
        var result = await _sender.Send(query, cancellationToken);
        
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
            request.Quantity
        );
        var cmd = new CreateProductCommand(dto);
        var result = await _sender.Send(cmd, cancellationToken);
        
        if (result.IsFailure)
            return NotFound(result.Errors);
        else
            return Ok();
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateProduct(ProductDto productDto, CancellationToken cancellationToken)
    {
        var cmd = new UpdateProductCommand(productDto);
        var result = await _sender.Send(cmd, cancellationToken);
        
        if(result.IsFailure)
            return BadRequest(result.Errors);
        return Ok();
    }
    
    [HttpDelete("{id:guid}")] 
    public async Task<ActionResult> DeleteProduct(Guid id)
    {
        var cmd = new DeleteProductCommand(ProductId.Create(id));
        
        var result = await _sender.Send(cmd);
        
        if(result.IsFailure)
            return NotFound(result.Errors);
        else
            return Ok();
    }

}
