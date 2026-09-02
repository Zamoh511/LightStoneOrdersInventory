using Dapper;
using Microsoft.Data.SqlClient;
using LightStoneOrdersInventory.Models;
using LightStoneOrdersInventory.DTOs;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LightStoneOrdersInventory.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public void AddProduct(Product product)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            conn.Execute("INSERT INTO Products (Sku, Name, Price, Stock) VALUES (@Sku, @Name, @Price, @Stock)", product);
        }

        public List<Product> GetProducts()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            var query = "SELECT Id, Sku, Name, Price, Stock FROM Products";
            var products = conn.Query<Product>(query).ToList();
            return products;
        }

        public async Task<bool> AdjustStockItemsAsync(IEnumerable<OrderItemDto> items)
        {
            if (items == null) return false;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                const string sql = "UPDATE Products SET Stock = Stock - @Qty WHERE Sku = @Sku AND Stock >= @Qty";
                foreach (var it in items)
                {
                    var rows = await conn.ExecuteAsync(sql, new { Sku = it.Sku, Qty = it.Qty }, tran);
                    if (rows == 0)
                    {
                        await tran.RollbackAsync();
                        return false;
                    }
                }

                await tran.CommitAsync();
                return true;
            }
            catch
            {
                try { await tran.RollbackAsync(); } catch { }
                throw;
            }
        }
    }
}
