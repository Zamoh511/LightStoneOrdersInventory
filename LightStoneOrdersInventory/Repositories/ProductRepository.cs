using Dapper;
using Microsoft.Data.SqlClient;
using LightStoneOrdersInventory.Models;
using LightStoneOrdersInventory.DTOs;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
            conn.Execute("INSERT INTO dbo.Products (Sku, Name, Price, AvailableStock) VALUES (@Sku, @Name, @Price, @Stock)", product);
        }

        public List<Product> GetProducts()
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            // Alias DB column names to the Product model property names so Dapper maps correctly
            var query = "SELECT ProductId AS Id, Sku, Name, Price, AvailableStock AS Stock FROM dbo.Products";
            var products = conn.Query<Product>(query).ToList();
            return products;
        }

        public async Task<bool> AdjustStockItemsAsync(IEnumerable<OrderItemDto> items)
        {
            if (items == null) return false;

            // Aggregate quantities per SKU in case same SKU appears multiple times
            var requiredBySku = items.GroupBy(i => i.Sku)
                                     .ToDictionary(g => g.Key, g => g.Sum(i => i.Qty));

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var tran = conn.BeginTransaction();

            try
            {
                // Acquire update locks on the target rows to prevent concurrent modifications
                var skus = requiredBySku.Keys.ToArray();
                var selectSql = "SELECT ProductId, Sku, AvailableStock FROM Products WITH (UPDLOCK, ROWLOCK) WHERE Sku IN @Skus";
                var products = (await conn.QueryAsync<Product>(selectSql, new { Skus = skus }, tran)).ToList();

                // Ensure all requested SKUs exist and have sufficient stock
                foreach (var kv in requiredBySku)
                {
                    var sku = kv.Key;
                    var needed = kv.Value;
                    var prod = products.SingleOrDefault(p => p.Sku == sku);
                    if (prod == null || prod.Stock < needed)
                    {
                        await tran.RollbackAsync();
                        return false;
                    }
                }

                // Perform the decrements (rows are locked so this is safe)
                const string updateSql = "UPDATE Products SET AvailableStock = AvailableStock - @Qty WHERE Sku = @Sku";
                foreach (var kv in requiredBySku)
                {
                    await conn.ExecuteAsync(updateSql, new { Sku = kv.Key, Qty = kv.Value }, tran);
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

        public async Task<List<SalesDayDto>> GetDailySalesAsync(DateTime start, DateTime end)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // Normalize dates to date-only range
            var startDate = start.Date;
            var endDate = end.Date.AddDays(1); // exclusive

            var sql = @"SELECT 
                            CAST(o.PlacedAt AS date) AS Day, oi.Sku, SUM(oi.Quantity) AS QtySold, SUM(oi.Quantity * oi.UnitPrice) AS GrossSales
                            FROM Orders o
                            INNER JOIN OrderItems oi ON oi.OrderId = o.Id
                            WHERE o.PlacedAt >= @StartDate AND o.PlacedAt < @EndDate AND o.Status = @AcceptedStatus
                            GROUP BY CAST(o.PlacedAt AS date), oi.Sku
                            ORDER BY CAST(o.PlacedAt AS date) ASC, oi.Sku ASC";

            var rows = await conn.QueryAsync(sql, new { StartDate = startDate, EndDate = endDate, AcceptedStatus = (int)OrderStatus.Accepted });

            // Transform into SalesDayDto list
            var grouped = rows.GroupBy(r => (DateTime)r.Day).Select(g =>
            {
                var products = g.Select(p => new SalesProductDto((string)p.Sku, (int)p.QtySold, (decimal)p.GrossSales)).ToList();
                var totalQty = products.Sum(p => p.QtySold);
                var totalGross = products.Sum(p => p.GrossSales);
                return new SalesDayDto(g.Key, products, totalQty, totalGross);
            }).ToList();

            return grouped;
        }


    }
}
