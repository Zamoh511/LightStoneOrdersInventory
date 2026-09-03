using Dapper;
using LightStoneOrdersInventory.Models;
using Microsoft.Data.SqlClient;

namespace LightStoneOrdersInventory.Repositories
{
    public class ReportsRepository: IReportsRepository
    {
        private readonly string _connectionString;

        public ReportsRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        public List<SalesReport> GetSalesReport(DateTime startDate, DateTime endDate)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
           
            var query = @"SELECT
                            OrderDate,
                            ProductId,
                            ProductName,
                            ProductQuantity,
                            ProductGrossAmount,
                            ProductOrderCount,
                            DayTotalQuantity,
                            DayTotalGrossAmount,
                            DayOrderCount
                            FROM dbo.vDailySalesByProduct
                          WHERE OrderDate >= @startDate
                            AND OrderDate <  @endDate
                          ORDER BY OrderDate, ProductName;";
            var report = conn.Query<SalesReport>(query,new {startDate =startDate, endDate = endDate }).ToList();
            return report;
        }
    }
}
