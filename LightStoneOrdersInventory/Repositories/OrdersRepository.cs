using Dapper;
using LightStoneOrdersInventory.Models;
using Microsoft.Data.SqlClient;

namespace LightStoneOrdersInventory.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly string _connectionString;

        public OrdersRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public void AddOrder(Order order)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            conn.Execute("INSERT INTO Orders (CustomerId, ProductId, Quantity) VALUES (@CustomerId, @ProductId, @Quantity)", order);
        }

        public List<Order> GetOrders()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT Id, CustomerId, ProductId, Quantity FROM Orders";
            var orders = conn.Query<Order>(sql).ToList();
            return orders;
        }
    }
}
