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

        public int AddOrder(Order order)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            return conn.QuerySingle<int>(@"BEGIN TRY 
                                INSERT INTO Orders (ExternalOrderId, PlacedAt, CreatedAt) 
                                VALUES (@ExternalOrderId, GETDATE(), GETDATE()) 
                                SELECT CAST(SCOPE_IDENTITY() AS INT) AS OrderId;
                                END TRY
                                BEGIN CATCH
                                    THROW;
                                END CATCH", order);
        }

        public void AddOrderItem(OrderItem orderItem)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            conn.Execute("INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)", orderItem);
        }

        public List<Order> GetOrders()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var sql = "SELECT Id, ExternalOrderId, PlacedAt, Status FROM Orders";
            var orders = conn.Query<Order>(sql).ToList();
            return orders;
        }

        public int AddOrderWithItems(Order order)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // Insert order and get id
                var orderId = conn.QuerySingle<int>(@"INSERT INTO Orders (ExternalOrderId, PlacedAt, CreatedAt) 
                                                    VALUES (@ExternalOrderId, GETDATE(), GETDATE()); 
                                                    SELECT CAST(SCOPE_IDENTITY() AS INT) AS OrderId;", order, tran);

                // Insert items
                const string itemSql = "INSERT INTO OrderItems (OrderId, ProductId, Quantity, UnitPrice) VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice)";
                foreach (var item in order.Items)
                {
                    item.OrderId = orderId;
                    conn.Execute(itemSql, item, tran);
                }

                tran.Commit();
                return orderId;
            }
            catch
            {
                try { tran.Rollback(); } catch { }
                throw;
            }
        }
    }
}
