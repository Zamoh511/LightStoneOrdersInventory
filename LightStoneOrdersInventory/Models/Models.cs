namespace LightStoneOrdersInventory.Models;

public enum OrderStatus
{
    Pending,
    Accepted,
    Rejected
}

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = null!;
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public string ExternalOrderId { get; set; } = null!;
    public DateTime PlacedAt { get; set; }
    public OrderStatus Status { get; set; }

    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public Order? Order { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public string Sku { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class SalesReport
{
    public DateTime OrderDate { get; set; }
    public int DayTotalQuantity { get; set; }
    public decimal DayTotalGrossAmount { get; set; }
    public int DayOrderCount { get; set; }
}
