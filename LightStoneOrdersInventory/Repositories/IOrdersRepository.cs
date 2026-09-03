using LightStoneOrdersInventory.Models;

namespace LightStoneOrdersInventory.Repositories
{
    public interface IOrdersRepository
    {
        int AddOrder(Order order);
        void AddOrderItem(OrderItem orderItem);
        // Insert order and its items atomically within a transaction and return the new OrderId
        int AddOrderWithItems(Order order);
        List <Order> GetOrders();
    }
        
}
