using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LightStoneOrdersInventory.Data;
using LightStoneOrdersInventory.DTOs;
using LightStoneOrdersInventory.Models;

namespace OrdersInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly OrdersDbContext _db;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(OrdersDbContext db, ILogger<ProductsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create(ProductCreateDto dto)
    {
        if (await _db.Products.AnyAsync(p => p.Sku == dto.Sku))
            return Conflict(new { error = "sku_exists" });

        var p = new Product { Sku = dto.Sku, Name = dto.Name, Price = dto.Price, Stock = dto.InitialStock };
        _db.Products.Add(p);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { sku = p.Sku }, p);
    }

    [HttpGet("{sku}")]
    public async Task<IActionResult> Get(string sku)
    {
        var p = await _db.Products.SingleOrDefaultAsync(x => x.Sku == sku);
        if (p == null) return NotFound();
        return Ok(p);
    }

    [HttpPatch("{sku}/stock")]
    public async Task<IActionResult> AdjustStock(string sku, StockAdjustDto dto)
    {
        var p = await _db.Products.SingleOrDefaultAsync(x => x.Sku == sku);
        if (p == null) return NotFound();
        p.Stock += dto.Delta;
        if (p.Stock < 0) p.Stock = 0;
        _db.Products.Update(p);
        await _db.SaveChangesAsync();
        return Ok(p);
    }
}
