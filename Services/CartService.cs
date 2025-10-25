using Ecommerce.Models;
using MongoDB.Driver;

namespace Ecommerce.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cart;

        public CartService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DoAnCuoiKy");
            _cart = database.GetCollection<Cart>("Carts");
        }

        // Lấy sản phẩm theo User 
        
        public async Task<Cart?> GetCart (string userId) => await _cart.Find(c => c.UserId == userId).FirstAsync();
        

    }
}
