using Ecommerce.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Services
{
    public class BillService
    {
        private readonly IMongoCollection<Bill> _bills;
        private readonly CartService _cartService;

        public BillService(CartService cartService)
        {
            _cartService = cartService;
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("DoAnCuoiKy");
            _bills = db.GetCollection<Bill>("Bills");
        }

        /// <summary>
        /// ✅ Tạo hóa đơn mới sau khi thanh toán thành công
        /// </summary>
        public async Task<bool> CreateBillAsync(string userId)
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);

            if (cart == null || cart.Items == null || !cart.Items.Any())
                return false;

            var bill = new Bill
            {
                UserId = userId,
                Items = cart.Items.Select(i => new BillItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = (decimal)i.Price
                }).ToList(),
                TotalAmount = cart.Items.Sum(i => (decimal)i.Price * i.Quantity),
                PaidAt = DateTime.UtcNow,
                Status = "Paid"
            };

            await _bills.InsertOneAsync(bill);

            //await _cartService.ClearCartAsync(userId);

            return true;
        }

        /// <summary>
        /// ✅ Lấy danh sách hóa đơn theo người dùng
        /// </summary>
        public async Task<List<Bill>> GetBillsByUserIdAsync(string userId)
        {
            return await _bills.Find(b => b.UserId == userId)
                               .SortByDescending(b => b.PaidAt)
                               .ToListAsync();
        }

        /// <summary>
        /// ✅ Lấy chi tiết 1 hóa đơn theo ID
        /// </summary>
        public async Task<Bill> GetBillByIdAsync(string id)
        {
            return await _bills.Find(b => b.Id == id).FirstOrDefaultAsync();
        }

        /// <summary>
        /// ✅ Cập nhật trạng thái hóa đơn
        /// </summary>
        public async Task UpdateBillStatusAsync(string billId, string status)
        {
            var update = Builders<Bill>.Update.Set(b => b.Status, status);
            await _bills.UpdateOneAsync(b => b.Id == billId, update);
        }

        /// <summary>
        /// ✅ Xóa hóa đơn theo ID
        /// </summary>
        public async Task DeleteBillAsync(string billId)
        {
            await _bills.DeleteOneAsync(b => b.Id == billId);
        }

        /// <summary>
        /// ✅ Lấy toàn bộ hóa đơn (dành cho Admin)
        /// </summary>
        public async Task<List<Bill>> GetAllBillsAsync()
        {
            return await _bills.Find(_ => true)
                               .SortByDescending(b => b.PaidAt)
                               .ToListAsync();
        }
    }
}
