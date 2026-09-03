namespace LightStoneOrdersInventory.DTOs;

public record ProductCreateDto(string Sku, string Name, decimal Price, int InitialStock);

public record StockAdjustDto(int Delta);

public record OrderItemDto(string Sku, int Qty, decimal UnitPrice);

public record OrderSubmitDto(string ExternalOrderId, DateTime PlacedAt, List<OrderItemDto> Items);

public record SalesProductDto(string Sku, int QtySold, decimal GrossSales);

public record SalesDayDto(DateTime Date, List<SalesProductDto> Products, int TotalQtySold, decimal TotalGrossSales);
