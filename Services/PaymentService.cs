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

        public async Task CreatePaymentAsync(Payment payment)
        {
            await _paymentCollection.InsertOneAsync(payment);
        }

        public async Task<List<Payment>> GetPaymentsByUserAsync(string userId)
        {
            return await _paymentCollection
                .Find(p => p.UserId == userId)
                .SortByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(string id)
        {
            return await _paymentCollection.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentCollection.Find(_ => true).ToListAsync();
        }

        public async Task<string> GeneratePayOSQrCodeAsync(string userId, decimal total)
        {
            await Task.Delay(500);
            return $"https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=ThanhToan_{userId}_{total}";
        }
    }
}
