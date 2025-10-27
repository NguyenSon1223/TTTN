using Ecommerce.Models;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _carts;
        private readonly ProductService _productService;

        public CartService(ProductService productService)
        {
            _productService = productService;
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("DoAnCuoiKy");
            _carts = db.GetCollection<Cart>("Carts");
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            var cart = await _carts.Find(c => c.UserId == userId).FirstOrDefaultAsync();

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid().ToString(), // ✅ tránh duplicate key
                    UserId = userId,
                    Items = new System.Collections.Generic.List<CartItem>(),
                    Status = "Pending"
                };
                await _carts.InsertOneAsync(cart);
            }

            // Cập nhật dữ liệu sản phẩm trong giỏ
            foreach (var item in cart.Items)
            {
                var p = await _productService.GetByIdAsync(item.ProductId);
                if (p != null)
                {
                    item.ProductName = p.Name;
                    item.ProductImage = p.ImageUrl;
                    item.Price = Convert.ToDouble(p.Price);
                }
            }

            return cart;
        }

        public async Task AddToCartAsync(string userId, string productId, int qty = 1)
        {
            var cart = await GetCartByUserIdAsync(userId);
            var exist = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (exist != null)
                exist.Quantity += qty;
            else
            {
                var p = await _productService.GetByIdAsync(productId);
                if (p != null)
                {
                    cart.Items.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = p.Name,
                        ProductImage = p.ImageUrl,
                        Price = Convert.ToDouble(p.Price),
                        Quantity = qty
                    });
                }
            }

            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }

        public async Task RemoveFromCartAsync(string userId, string productId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            cart.Items.RemoveAll(i => i.ProductId == productId);
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            cart.Items.Clear();
            await _carts.ReplaceOneAsync(c => c.Id == cart.Id, cart);
        }

        public async Task UpdateCartStatusAsync(string cartId, string status)
        {
            var update = Builders<Cart>.Update.Set(c => c.Status, status);
            await _carts.UpdateOneAsync(c => c.Id == cartId, update);
        }

        public async Task UpdateCartStatusByUserAsync(string userId, string status)
        {
            var update = Builders<Cart>.Update.Set(c => c.Status, status);
            await _carts.UpdateOneAsync(c => c.UserId == userId, update);
        }
    }
}
