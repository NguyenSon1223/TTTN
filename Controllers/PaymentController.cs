using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly CartService _cartService;
        private readonly PayOS _payOS;
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(CartService cartService, IConfiguration config, EmailService emailSerivce, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;

            var clientId = config["PayOS:ClientId"];
            var apiKey = config["PayOS:ApiKey"];
            var checksumKey = config["PayOS:ChecksumKey"];
            _payOS = new PayOS(clientId, apiKey, checksumKey);
            _emailService = emailSerivce;
            _userManager = userManager;
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
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var paymentData = new PaymentData(
                orderCode,
                (int)cart.Items.Sum(i => i.Price * i.Quantity),
                "Thanh toán đơn hàng",
                items,
                $"{baseUrl}/payment/cancel",
                $"{baseUrl}/payment/success"
            );

            var paymentLink = await _payOS.createPaymentLink(paymentData);

            // ✅ Cập nhật trạng thái giỏ hàng
            await _cartService.UpdateCartStatusByUserAsync(userId, "Paid");

            return Redirect(paymentLink.checkoutUrl);
        }

        public IActionResult Cancel ()
        {
            return View();
        }
        
        public async Task<IActionResult> Success()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            var user = await _userManager.FindByIdAsync(userId);
            var toEmail = user?.Email; // <-- lấy email chuẩn

            if (string.IsNullOrWhiteSpace(toEmail) || !toEmail.Contains("@"))
            {
                TempData["Error"] = "Không có địa chỉ email hợp lệ để gửi xác nhận.";
                // log hoặc xử lý khác nếu muốn
                return View(); // hoặc RedirectToAction("Index","Home")
            }

            var subject = "Xác nhận thanh toán";
            var body = "<p>Cảm ơn bạn... đơn hàng đã thanh toán.</p>";

            try
            {
                await _emailService.SendEmailAsync(toEmail, subject, body);
                TempData["Success"] = "Email xác nhận đã gửi.";
            }
            catch (Exception ex)
            {
                // log ex.Message
                TempData["Error"] = "Gửi email thất bại: " + ex.Message;
            }

            return View();
        }
    }
}
