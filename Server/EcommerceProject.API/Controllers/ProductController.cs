using EcommerceProject.API.Http.Product.Requests;
using EcommerceProject.API.Http.Product.Responses;
using EcommerceProject.Application.Dto.Product;
using EcommerceProject.Application.UseCases.Products.Commands.CreateProduct;
using EcommerceProject.Application.UseCases.Products.Commands.DeleteProduct;
using EcommerceProject.Application.UseCases.Products.Commands.UpdateProduct;
using EcommerceProject.Application.UseCases.Products.Queries.GetProductById;
using EcommerceProject.Application.UseCases.Products.Queries.GetProducts;
using EcommerceProject.Core.Models.Products.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceProject.API.Controllers;

[Route("api/products")]
public class ProductController : ApiController
{
    public ProductController(ISender sender) : base(sender)
    {
    }

    [HttpGet]
    public async Task<ActionResult<GetProductsResponse>> GetProducts(CancellationToken cancellationToken)
    {
        var query = new GetProductsQuery();
        var result = await Sender.Send(query, cancellationToken);

        return result.Map<ActionResult<GetProductsResponse>>(
            onSuccess: value => Ok(new GetProductsResponse(value)),
            onFailure: errors => NotFound(errors));
    }

    [HttpGet(template: "{id:guid}")]
    public async Task<ActionResult<ProductReadDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(Id: ProductId.Create(productId: id).Value);
        var result = await Sender.Send(request: query, cancellationToken: cancellationToken);

        return result.Map<ActionResult<ProductReadDto>>(
            onSuccess: value => Ok(value),
            onFailure: errors => NotFound(value: errors));

    }

    [HttpPost]
    public async Task<ActionResult<Guid>> AddProduct([FromBody] AddProductRequest request,
        CancellationToken cancellationToken)
    {
        ProductWriteDto writeDto = new(
            Id: Guid.NewGuid(),
            Title: request.Title,
            Description: request.Description,
            Price: request.Price,
            Quantity: request.Quantity,
            CategoryId: request.CategoryId
        );
        var cmd = new CreateProductCommand(writeDto);
        var result = await Sender.Send(cmd, cancellationToken);

        return result.Map<ActionResult<Guid>>(
            onSuccess: () => Ok(writeDto.Id),
            onFailure: errors => BadRequest(errors));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        ProductWriteDto writeDto = new ProductWriteDto(
            Id: request.Id,
            Title: request.Title,
            Description: request.Description,
            Price: request.Price,
            Quantity: request.Quantity,
            CategoryId: request.CategoryId
        );

        var cmd = new UpdateProductCommand(writeDto);
        var result = await Sender.Send(cmd, cancellationToken);

        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => BadRequest(errors));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var cmd = new DeleteProductCommand(ProductId.Create(id).Value);

        var result = await Sender.Send(cmd);

        return result.Map<IActionResult>(
            onSuccess: () => Ok(),
            onFailure: errors => BadRequest(errors));
    }
}