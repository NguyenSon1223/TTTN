using Ecommerce.Models;
using MongoDB.Driver;

namespace Ecommerce.Services
{
    public class ProductService
    {
        private readonly IMongoCollection<Product> _products;

        public ProductService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DoAnCuoiKy");
            _products = database.GetCollection<Product>("Products");
        }

        // Lấy tất cả sản phẩm
        public async Task<List<Product>> GetAllAsync() =>
            await _products.Find(_ => true).ToListAsync();

        // Lấy sản phẩm theo ID
        public async Task<Product?> GetByIdAsync(string id) =>
            await _products.Find(p => p.Id == id).FirstOrDefaultAsync();

        // Tạo mới sản phẩm
        public async Task CreateAsync(Product product) =>
            await _products.InsertOneAsync(product);

        // Cập nhật sản phẩm
        public async Task UpdateAsync(string id, Product updatedProduct) =>
            await _products.ReplaceOneAsync(p => p.Id == id, updatedProduct);

        // Xóa sản phẩm
        public async Task DeleteAsync(string id) =>
            await _products.DeleteOneAsync(p => p.Id == id);

        // ✅ Lọc theo Category
        public async Task<List<Product>> GetByCategoryAsync(string category) =>
            await _products.Find(p => p.Category == category).ToListAsync();
    }
}
