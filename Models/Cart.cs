using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class Cart
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("UserId")]
        public string UserId { get; set; }
        [BsonElement("ProductId")]
        public string ProductId { get; set; }
        [BsonElement("Price")]
        public double price { get; set; }
    }
}
