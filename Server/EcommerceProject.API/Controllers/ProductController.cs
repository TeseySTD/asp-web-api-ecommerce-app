using Microsoft.AspNetCore.Mvc;
using EcommerceProject.Core;
using EcommerceProject.Core.Models;
using EcommerceProject.API.Contracts;
using System.Drawing;
using System.Reflection.Metadata.Ecma335;
namespace EcommerceProject.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    // private readonly IProductService _productService;
    //
    // public ProductController(IProductService productService)
    // {
    //     _productService = productService;
    // }
    //
    // [HttpGet]
    // public async Task<ActionResult<List<ProductResponse>>> GetProducts()
    // {
    //     var products = await _productService.GetAllProducts();
    //     var response = products.Select(p => new ProductResponse(p.Id, p.Title, p.Description, p.Price));
    //     return Ok(response);
    // }
    //
    // [HttpGet("{id:guid}")] 
    // public async Task<ActionResult<ProductResponse>> GetProductById(Guid id)
    // {
    //     try
    //     {
    //         var product = await _productService.FindProductById(id);
    //         if (product == null)
    //             return NotFound();
    //         return Ok(new ProductResponse(product.Id, product.Title, product.Description, product.Price));
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    // }
    //
    // [HttpPost]
    // public async Task<ActionResult<Guid>> AddProduct([FromBody] ProductRequest productRequest)
    // {
    //     Product product;
    //     try
    //     {
    //         product = Product.Create(productRequest.Title, productRequest.Description, productRequest.Price);
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    //
    //     await _productService.AddProduct(product);
    //     return Ok(product.Id);
    // }
    //
    //
    // [HttpPut("{id:guid}")]
    // public async Task<ActionResult<Guid>> UpdateProduct(Guid id, ProductRequest productRequest)
    // {
    //     try
    //     {
    //         await _productService.UpdateProduct(Product.Create(id, productRequest.Title, productRequest.Description, productRequest.Price));
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    //     return Ok(id);
    // }
    //
    // [HttpDelete("{id:guid}")] 
    // public async Task<ActionResult<Guid>> DeleteProduct(Guid id)
    // {
    //     try
    //     {
    //         await _productService.DeleteProduct(id);
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(ex.Message);
    //     }
    //     return Ok(id);
    // }

}
