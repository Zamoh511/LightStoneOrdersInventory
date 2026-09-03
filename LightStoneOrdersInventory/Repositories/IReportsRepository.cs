using LightStoneOrdersInventory.Models;

namespace LightStoneOrdersInventory.Repositories
{
    public interface IReportsRepository
    {
        List<SalesReport> GetSalesReport(DateTime startDate, DateTime endDate);
    }
}
