using Ecommerce.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ecommerce.Services
{
    public class PaymentService
    {
        private readonly IMongoCollection<Payment> _paymentCollection;

        public PaymentService(IConfiguration config)
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("DoAnCuoiKy");
            _paymentCollection = db.GetCollection<Payment>("Payments");
        }

        // ✅ Tạo mới thanh toán (hóa đơn)
        public async Task CreatePaymentAsync(Payment payment)
        {
            await _paymentCollection.InsertOneAsync(payment);
        }

        // ✅ Lấy danh sách thanh toán của 1 user
        public async Task<List<Payment>> GetPaymentsByUserAsync(string userId)
        {
            return await _paymentCollection
                .Find(p => p.UserId == userId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        // ✅ Lấy chi tiết 1 thanh toán theo ID
        public async Task<Payment?> GetPaymentByIdAsync(string id)
        {
            return await _paymentCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        // ✅ Lấy tất cả thanh toán (chỉ dùng cho Admin)
        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentCollection.Find(_ => true).ToListAsync();
        }

        public async Task<string> GeneratePayOSQrCodeAsync(string userId, decimal total)
        {
            // Giả lập gọi API PayOS — thực tế bạn sẽ dùng HttpClient để gửi request
            await Task.Delay(500); // mô phỏng xử lý

            // Trả về URL ảnh QR mẫu
            return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=ThanhToan_{userId}_{total}";
        }
    }
}
