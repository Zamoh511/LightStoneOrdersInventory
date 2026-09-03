using LightStoneOrdersInventory.Models;

namespace LightStoneOrdersInventory.Repositories
{
    public interface IProductRepository
    {
        void AddProduct(Product product);
        List<Product> GetProducts();
        Task<bool> AdjustStockItemsAsync(IEnumerable<LightStoneOrdersInventory.DTOs.OrderItemDto> items);
        Task<List<DTOs.SalesDayDto>> GetDailySalesAsync(System.DateTime start, System.DateTime end);
    }
        
}
