using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly CartService _cartService;
        private readonly PayOS _payOS;

        public PaymentController(CartService cartService, IConfiguration config)
        {
            _cartService = cartService;

            var clientId = config["PayOS:ClientId"];
            var apiKey = config["PayOS:ApiKey"];
            var checksumKey = config["PayOS:ChecksumKey"];
            _payOS = new PayOS(clientId, apiKey, checksumKey);
        }

        [HttpGet("Payment/Create")]
        public async Task<IActionResult> Create()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var items = cart.Items.Select(i => new ItemData(i.ProductName, i.Quantity, (int)i.Price)).ToList();

            var paymentData = new PaymentData(
                orderCode,
                (int)cart.Items.Sum(i => i.Price * i.Quantity),
                "Thanh toán đơn hàng",
                items,
                "https://yourdomain.com/payment-cancel",
                "https://yourdomain.com/payment-success"
            );

            var paymentLink = await _payOS.createPaymentLink(paymentData);

            // ✅ Cập nhật trạng thái giỏ hàng
            await _cartService.UpdateCartStatusByUserAsync(userId, "Paid");

            return Redirect(paymentLink.checkoutUrl);
        }
    }
}
