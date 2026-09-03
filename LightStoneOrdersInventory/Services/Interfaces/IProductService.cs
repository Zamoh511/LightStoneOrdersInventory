namespace LightStoneOrdersInventory.Services.Interfaces
{
    public interface IProductService
    {
        void AddProduct(Models.Product product);
        List<Models.Product> GetProducts();
        Task<bool> AdjustStockItemsAsync(IEnumerable<DTOs.OrderItemDto> items);
    }
}
