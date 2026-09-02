using Microsoft.AspNetCore.Mvc;
using LightStoneOrdersInventory.DTOs;
using LightStoneOrdersInventory.Services;

namespace LightStoneOrdersInventory.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _svc;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService svc, ILogger<OrdersController> logger)
    {
        _svc = svc;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Submit(OrderSubmitDto dto)
    {
        _logger.LogInformation("Received order submission {ExternalOrderId}", dto.ExternalOrderId);
        var (accepted, reason) = await _svc.SubmitOrderAsync(dto);
        if (reason == "duplicate")
            return Ok(new { result = "duplicate" });
        if (!accepted && reason == "insufficient_stock")
            return Conflict(new { result = "insufficient_stock" });

        return CreatedAtAction(null, new { result = "accepted" });
    }
}
