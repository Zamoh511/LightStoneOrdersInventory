using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LightStoneOrdersInventory.Data;
using LightStoneOrdersInventory.DTOs;
using LightStoneOrdersInventory.Models;
using LightStoneOrdersInventory.Services.Interfaces;

namespace LightStoneOrdersInventory.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    
    private readonly ILogger<ProductsController> _logger;
    private readonly IProductService _productService;

    public ProductsController( ILogger<ProductsController> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        if(dto == null || string.IsNullOrWhiteSpace(dto.Sku) || string.IsNullOrWhiteSpace(dto.Name) || dto.Price < 0)
        {
            return BadRequest(new { error = "invalid_request" });
        }
        try
        {
            var product = new Product
            {
                Sku = dto.Sku,
                Name = dto.Name,
                Price = dto.Price,
            };
            _productService.AddProduct(product);
            return CreatedAtAction(nameof(Get), new { sku = product.Sku }, product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return BadRequest(new { error = "invalid_request" });
        }
    }

    [HttpGet("{sku}")]
    public async Task<IActionResult> Get(string sku)
    {
        return Ok(_productService.GetProducts().FirstOrDefault(p => p.Sku == sku));
    }

    [HttpPatch("{sku}/stock")]
    public async Task<IActionResult> AdjustStock(string sku, IEnumerable<OrderItemDto> items)
    {

        await _productService.AdjustStockItemsAsync(items);
        return Ok();
    }
}
