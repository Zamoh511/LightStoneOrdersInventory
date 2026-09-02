using Microsoft.AspNetCore.Mvc;

using LightStoneOrdersInventory.Data;
using LightStoneOrdersInventory.DTOs;

namespace LightStoneOrdersInventory.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly OrdersDbContext _db;

    public SalesController(OrdersDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime start, [FromQuery] DateTime end)
    {
        // Normalize dates to UTC dates
        var startDate = start.Date;
        var endDate = end.Date;

        var itemsQuery = from o in _db.Orders
                         where o.Status == Models.OrderStatus.Accepted
                            && o.PlacedAt >= startDate && o.PlacedAt <= endDate.AddDays(1).AddTicks(-1)
                         from oi in o.Items
                         select new { Day = EF.Functions.DateFromParts(o.PlacedAt.Year, o.PlacedAt.Month, o.PlacedAt.Day), oi.Sku, oi.Quantity, oi.UnitPrice };

        // Note: EF.Functions.DateFromParts not supported in all versions; fallback to cast
        var grouped = await _db.OrderItems
            .Where(oi => oi.Order!.Status == Models.OrderStatus.Accepted && oi.Order.PlacedAt >= startDate && oi.Order.PlacedAt <= endDate.AddDays(1).AddTicks(-1))
            .Select(oi => new { Day = oi.Order!.PlacedAt.Date, oi.Sku, oi.Quantity, oi.UnitPrice })
            .GroupBy(x => new { x.Day, x.Sku })
            .Select(g => new
            {
                g.Key.Day,
                g.Key.Sku,
                QtySold = g.Sum(x => x.Quantity),
                GrossSales = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .ToListAsync();

        var days = grouped.GroupBy(x => x.Day).Select(dg =>
        {
            var products = dg.Select(p => new SalesProductDto(p.Sku, p.QtySold, p.GrossSales)).ToList();
            var totalQty = products.Sum(p => p.QtySold);
            var totalGross = products.Sum(p => p.GrossSales);
            return new SalesDayDto(dg.Key, products, totalQty, totalGross);
        }).OrderBy(d => d.Date).ToList();

        return Ok(new { start = startDate, end = endDate, days });
    }
}
