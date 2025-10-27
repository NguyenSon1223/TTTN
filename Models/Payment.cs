using System;
using System.Collections.Generic;

namespace Ecommerce.Models
{
    public class Payment
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
