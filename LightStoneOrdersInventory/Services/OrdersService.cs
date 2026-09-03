using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LightStoneOrdersInventory.Data;
using LightStoneOrdersInventory.DTOs;
using LightStoneOrdersInventory.Models;
using LightStoneOrdersInventory.Repositories;
using LightStoneOrdersInventory.Services.Interfaces;

namespace LightStoneOrdersInventory.Services;



public class OrdersService : IOrdersService
{
    private readonly ILogger<OrdersService> _logger; 
    private readonly IOrdersRepository _ordersRepository;
    private readonly IProductService _productService;
    private readonly IReportsRepository _reportsRepository;
 

    public OrdersService(ILogger<OrdersService> logger, IOrdersRepository ordersRepository, IProductService productService, IReportsRepository reportsRepository)
    {
        _logger = logger;
        _ordersRepository = ordersRepository;
        _productService = productService;
        _reportsRepository = reportsRepository;
    }

    public void AddOrder(Order order)
    {
        if (order == null) throw new ArgumentNullException(nameof(order));
        if (order.Items == null || !order.Items.Any()) throw new ArgumentException("Order must contain at least one item", nameof(order));

        var products = _productService.GetProducts().ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);

        var Order = new Order
        {
            ExternalOrderId = order.ExternalOrderId,
            PlacedAt = order.PlacedAt,
            Status = order.Status,
            Items = order.Items.Select(i =>
            {
                if (!products.TryGetValue(i.Sku, out var prod))
                {
                    throw new ArgumentException($"Unknown SKU: {i.Sku}");
                }

                return new OrderItem
                {
                    ProductId = prod.Id,
                    Sku = i.Sku,
                    Quantity = i.Quantity,
                    UnitPrice = prod.Price
                };
            }).ToList()
        };

        // Insert order and items atomically
        var orderId = _ordersRepository.AddOrderWithItems(Order);
    }
    public void AddOrderItem(OrderItem orderItem)
    {
        if (orderItem == null) throw new ArgumentNullException(nameof(orderItem));
        if (orderItem.Quantity <= 0) throw new ArgumentException("Quantity must be greater than zero", nameof(orderItem));
        var OrderItem = new OrderItem
        {
            ProductId = orderItem.ProductId,
            Sku = orderItem.Sku,
            Quantity = orderItem.Quantity,
            UnitPrice = orderItem.UnitPrice
        };
        _ordersRepository.AddOrderItem(OrderItem);
    }

    public IEnumerable<SalesReport> GetSalesReport(DateTime startDate, DateTime endDate)
    {
        return _reportsRepository.GetSalesReport(startDate, endDate);
    }

}
