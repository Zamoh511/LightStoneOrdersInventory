using LightStoneOrdersInventory.Models;

namespace LightStoneOrdersInventory.Repositories
{
    public interface IOrdersRepository
    {
        void AddOrder(Order order);

        List<Order> GetOrders();
    }
        
}
