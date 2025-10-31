using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class Bill
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public List<BillItem> Items { get; set; } = new List<BillItem>();

        public decimal TotalAmount { get; set; }

        public DateTime PaidAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "Paid";
    }
}
