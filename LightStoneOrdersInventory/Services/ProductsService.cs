using LightStoneOrdersInventory.Models;
using LightStoneOrdersInventory.Repositories;
using LightStoneOrdersInventory.Services.Interfaces;

namespace LightStoneOrdersInventory.Services
{
    public class ProductsService : IProductService
    {
        private readonly IProductRepository _productRepository;
        public ProductsService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        public void AddProduct(Product product)
        { 
            _productRepository.AddProduct(product);
        }

        public List<Product> GetProducts()
        {
            return _productRepository.GetProducts();
        }

        public Task<bool> AdjustStockItemsAsync(IEnumerable<DTOs.OrderItemDto> items)
        {
            return _productRepository.AdjustStockItemsAsync(items);
        }
    }
}
