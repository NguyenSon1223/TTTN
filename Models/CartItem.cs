using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ecommerce.Models
{
    public class CartItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductImage { get; set; } = string.Empty; // tên file trong wwwroot/img hoặc url
        public int Quantity { get; set; } = 1;
        public double Price { get; set; } = 0;
    }
}
