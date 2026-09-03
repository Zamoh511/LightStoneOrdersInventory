using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using LightStoneOrdersInventory.Data;
using LightStoneOrdersInventory.DTOs;
using LightStoneOrdersInventory.Models;
using LightStoneOrdersInventory.Services;
using LightStoneOrdersInventory.Services.Interfaces;

namespace LightStoneOrdersInventory.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly IOrdersService _ordersService;
    public SalesController(IOrdersService ordersService)
    {
       _ordersService = ordersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var salesReports = _ordersService.GetSalesReport(startDate, endDate);
            return Ok(salesReports);        
        }
        catch
        {
            return BadRequest(new { error = "invalid_request" });
        }
       
    }
}
