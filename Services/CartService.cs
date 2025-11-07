using Ecommerce.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Services
{
    public class CartService
    {
        private readonly IMongoCollection<Cart> _cartCollection;
        private readonly IMongoCollection<Product> _productCollection;

        public CartService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("DoAnCuoiKy");
            _cartCollection = database.GetCollection<Cart>("Carts");
            _productCollection = database.GetCollection<Product>("Products");
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            var cart = await _cartCollection.Find(c => c.UserId == userId).FirstOrDefaultAsync();
            return cart;
        }

        public async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>(),
                    Status = "Pending"
                };
                await _cartCollection.InsertOneAsync(cart);
            }
            return cart;
        }

        public async Task AddToCartAsync(string userId, string productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var product = await _productCollection.Find(p => p.Id == productId).FirstOrDefaultAsync();

            if (product == null)
                throw new Exception("Sản phẩm không tồn tại.");

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = quantity,
                    Price = (double)product.Price
                });
            }

            var filter = Builders<Cart>.Filter.Eq(c => c.Id, cart.Id);
            await _cartCollection.ReplaceOneAsync(filter, cart);
        }

        public async Task UpdateQuantityAsync(string userId, string productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                item.Quantity = quantity;
                var filter = Builders<Cart>.Filter.Eq(c => c.Id, cart.Id);
                await _cartCollection.ReplaceOneAsync(filter, cart);
            }
        }

        public async Task RemoveFromCartAsync(string userId, string productId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            cart.Items.RemoveAll(i => i.ProductId == productId);

            var filter = Builders<Cart>.Filter.Eq(c => c.Id, cart.Id);
            await _cartCollection.ReplaceOneAsync(filter, cart);
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart != null)
            {
                cart.Items.Clear();
                cart.Status = "Empty";
                var filter = Builders<Cart>.Filter.Eq(c => c.Id, cart.Id);
                await _cartCollection.ReplaceOneAsync(filter, cart);
            }
        }

        public async Task UpdateCartStatusByUserAsync(string userId, string status)
        {
            var filter = Builders<Cart>.Filter.Eq(c => c.UserId, userId);
            var update = Builders<Cart>.Update.Set(c => c.Status, status);
            await _cartCollection.UpdateOneAsync(filter, update);
        }
    }
}
