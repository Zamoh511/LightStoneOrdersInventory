using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using LightStoneOrdersInventory.Data;
using LightStoneOrdersInventory.DTOs;
using LightStoneOrdersInventory.Models;
using LightStoneOrdersInventory.Repositories;

namespace LightStoneOrdersInventory.Services;

public interface IOrderService
{
    Task<(bool Accepted, string Reason)> SubmitOrderAsync(OrderSubmitDto dto);
}

public class OrderService : IOrderService
{
    private readonly OrdersDbContext dbContext;
    private readonly ILogger<OrderService> _logger; 
    private readonly IOrdersRepository _ordersRepository;

    public OrderService(OrdersDbContext _dbContext, ILogger<OrderService> logger, IOrdersRepository ordersRepository)
    {
        _dbContext = dbContext;
        _logger = logger;
        _ordersRepository = ordersRepository;
    }

    public async Task<(bool Accepted, string Reason)> SubmitOrderAsync(OrderSubmitDto dto)
    {
        if (dto == null)
        {
            return (false, "invalid_request");
        }
        return   (true, "accepted"); ;
    }
}
