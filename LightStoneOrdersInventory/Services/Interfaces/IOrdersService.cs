using LightStoneOrdersInventory.Models;

namespace LightStoneOrdersInventory.Services.Interfaces
{
    public interface IOrdersService
    {
        void AddOrder(Order order);
        void AddOrderItem(OrderItem orderItem);

        IEnumerable<SalesReport> GetSalesReport(DateTime startDate, DateTime endDate);
    }
}
