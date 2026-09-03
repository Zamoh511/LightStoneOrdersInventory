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
                                MAX(DayTotalQuantity)    AS DayTotalQuantity,
                                MAX(DayTotalGrossAmount) AS DayTotalGrossAmount,
                                MAX(DayOrderCount)       AS DayOrderCount
                            FROM dbo.vDailySalesByProduct
                            WHERE OrderDate >= @startDate
                              AND OrderDate <= @endDate
                            GROUP BY OrderDate
                            ORDER BY OrderDate;";
            var report = conn.Query<SalesReport>(query,new {startDate =startDate, endDate = endDate }).ToList();
            return report;
        }
    }
}
