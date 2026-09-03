using Microsoft.AspNetCore.Mvc;
using LightStoneOrdersInventory.DTOs;
using LightStoneOrdersInventory.Services;
using LightStoneOrdersInventory.Services.Interfaces;
using LightStoneOrdersInventory.Models;

namespace LightStoneOrdersInventory.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _ordersService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(ILogger<OrdersController> logger, IOrdersService ordersService)
    {
        _logger = logger;
        _ordersService = ordersService;
        
    }

    [HttpPost]
    public async Task<IActionResult> Create(OrderSubmitDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.ExternalOrderId) || dto.Items == null || !dto.Items.Any())
        {
            return BadRequest(new { error = "invalid_request" });
        }
        try
        {
            var order = new Order
            {
                ExternalOrderId = dto.ExternalOrderId,
                Items = dto.Items.Select(i => new OrderItem
                {
                    Sku = i.Sku,
                    Quantity = i.Qty
                }).ToList()
            };

            _ordersService.AddOrder(order);
            return Ok(); //CreatedAtAction(nameof(order), new { ExternalOrderId = order.ExternalOrderId }, order);
        }
        catch (ArgumentException aex) when (aex.Message != null && aex.Message.StartsWith("Unknown SKU:"))
        {
            // Return a clear client error for unknown SKU
            _logger.LogWarning(aex, "Order contains unknown SKU");
            var parts = aex.Message.Split(':', 2);
            var sku = parts.Length > 1 ? parts[1].Trim() : string.Empty;
            return BadRequest(new { error = "unknown_sku", sku });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return BadRequest(new { error = "invalid_request" });
        }
    }

    //[HttpPost]
    //public async Task<IActionResult> Submit(OrderSubmitDto dto)
    //{
    //    _logger.LogInformation("Received order submission {ExternalOrderId}", dto.ExternalOrderId);

    //    //var (accepted, reason) = await _ordersService.SubmitOrderAsync(dto);
    //    //if (reason == "duplicate")
    //    //    return Ok(new { result = "duplicate" });
    //    //if (!accepted && reason == "insufficient_stock")
    //    //    return Conflict(new { result = "insufficient_stock" });

    //    return CreatedAtAction(null, new { result = "accepted" });
    //}
}
