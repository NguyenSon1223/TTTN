using Ecommerce.Models;
using MongoDB.Driver;

namespace Ecommerce.Seeder
{
    public static class DataSeeder
    {
        public static void Seed(IServiceProvider serviceProvider)
        {
            var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
            var database = mongoClient.GetDatabase("EcommerceDB");

            // Lấy collection sản phẩm
            var productsCollection = database.GetCollection<Product>("Products");

            // Nếu chưa có sản phẩm nào thì seed dữ liệu mẫu
            if (!productsCollection.Find(_ => true).Any())
            {
                var sampleProducts = new List<Product>
                {
                    new Product { Name = "Áo thun nam", Price = 199000, Description = "Áo thun cotton 100%", ImageUrl = "/img/products/shirt1.jpg", Category = "Thời trang nam" },
                    new Product { Name = "Giày thể thao nữ", Price = 499000, Description = "Giày thể thao nhẹ, thoáng khí", ImageUrl = "/img/products/shoes1.jpg", Category = "Thời trang nữ" },
                    new Product { Name = "Tai nghe Bluetooth", Price = 299000, Description = "Tai nghe không dây âm thanh nổi", ImageUrl = "/img/products/headphone1.jpg", Category = "Phụ kiện" },
                    new Product { Name = "Túi xách da", Price = 899000, Description = "Túi xách da thật cao cấp", ImageUrl = "/img/products/bag1.jpg", Category = "Thời trang nữ" }
                };

                productsCollection.InsertMany(sampleProducts);
                Console.WriteLine("✅ Đã seed dữ liệu mẫu cho Products!");
            }
        }
    }
}
