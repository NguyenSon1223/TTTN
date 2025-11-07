using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly EmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly BillService _billService;

        public PaymentController(
            CartService cartService,
            IConfiguration config,
            EmailService emailService,
            UserManager<ApplicationUser> userManager,
            BillService billService)
        {
            _cartService = cartService;
            _billService = billService;

            var clientId = config["PayOS:ClientId"];
            var apiKey = config["PayOS:ApiKey"];
            var checksumKey = config["PayOS:ChecksumKey"];
            _payOS = new PayOS(clientId, apiKey, checksumKey);

            _emailService = emailService;
            _userManager = userManager;
        }

        [HttpGet("Payment/Create")]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var userId = user.Id.ToString();

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
                return RedirectToAction("Index", "Cart");

            var orderCode = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var items = cart.Items.Select(i =>
                new ItemData(i.ProductName, i.Quantity, (int)i.Price)).ToList();

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

            await _cartService.UpdateCartStatusByUserAsync(userId, "Processing");

            return Redirect(paymentLink.checkoutUrl);
        }

        public IActionResult Cancel()
        {
            return View();
        }

        [HttpGet("/payment/success")]
        public async Task<IActionResult> Success()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "Bạn cần đăng nhập để xem hóa đơn.";
                    return RedirectToAction("Login", "Account");
                }

                var userId = user.Id.ToString();

                var cart = await _cartService.GetCartByUserIdAsync(userId);
                if (cart == null || cart.Items == null || !cart.Items.Any())
                {
                    TempData["Error"] = "Không tìm thấy giỏ hàng hoặc giỏ hàng trống.";
                    return RedirectToAction("Index", "Cart");
                }

                await _cartService.UpdateCartStatusByUserAsync(userId, "Paid");
                var success = await _billService.CreateBillAsync(userId);

                if (!success)
                {
                    TempData["Error"] = "Không thể tạo hóa đơn từ giỏ hàng.";
                    return RedirectToAction("Index", "Cart");
                }

                var bills = await _billService.GetBillsByUserIdAsync(userId);
                var bill = bills.FirstOrDefault();

                await _cartService.ClearCartAsync(userId);

                if (bill == null)
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn sau khi tạo.";
                    return RedirectToAction("Index", "Cart");
                }

                try
                {
                    var subject = "🎉 Xác nhận thanh toán thành công!";
                    var body = $@"
                    <div style='font-family:Segoe UI,Arial,sans-serif;background:#f4f6f8;padding:30px;'>
                        <div style='max-width:600px;margin:auto;background:white;border-radius:12px;padding:25px;box-shadow:0 3px 10px rgba(0,0,0,0.1);'>
                            <h2 style='color:#2f855a;text-align:center;'>Thanh toán thành công 💚</h2>
                            <p style='font-size:16px;text-align:center;color:#333;'>Cảm ơn bạn đã thực hiện thanh toán!</p>
                            <hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>
                            <div style='text-align:center;'>
                                <p style='font-size:15px;'>Ngày thanh toán: <b>{bill.PaidAt:dd/MM/yyyy HH:mm}</b></p>
                                <p style='font-size:15px;'>Tổng tiền: <b>{bill.TotalAmount:N0}₫</b></p>
                                <p style='font-size:15px;'>Sản phẩm: <b>{bill.Items.Count}</b> mặt hàng</p>
                            </div>
                            <hr style='border:none;border-top:1px solid #eee;margin:20px 0;'/>
                            <table style='width:100%;border-collapse:collapse;'>
                                <thead>
                                    <tr style='background:#f8f9fa;text-align:left;'>
                                        <th style='padding:8px;'>Sản phẩm</th>
                                        <th style='padding:8px;'>Số lượng</th>
                                        <th style='padding:8px;'>Giá</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {string.Join("", bill.Items.Select(i => $@"
                                        <tr>
                                            <td style='padding:8px;border-bottom:1px solid #eee;'>{i.ProductName}</td>
                                            <td style='padding:8px;border-bottom:1px solid #eee;text-align:center;'>{i.Quantity}</td>
                                            <td style='padding:8px;border-bottom:1px solid #eee;text-align:right;'>{i.Price:N0}₫</td>
                                        </tr>
                                    "))}
                                </tbody>
                            </table>
                            <p style='text-align:center;margin-top:20px;'>
                                <a href='https://yourdomain.com/bills' style='background:#2f855a;color:white;padding:10px 20px;border-radius:6px;text-decoration:none;'>Xem hóa đơn của bạn</a>
                            </p>
                            <p style='text-align:center;font-size:12px;color:#aaa;margin-top:20px;'>© 2025 - Ecommerce Store<br/>Email này được gửi tự động — vui lòng không phản hồi lại.</p>
                        </div>
                    </div>";

                    if (!string.IsNullOrWhiteSpace(user.Email))
                        await _emailService.SendEmailAsync(user.Email, subject, body);
                }
                catch (Exception emailEx)
                {
                    Console.WriteLine("[Email Error] " + emailEx.Message);
                }

                return View("Success", bill);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[Payment Error] " + ex.Message);
                TempData["Error"] = "Lỗi khi xử lý thanh toán: " + ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}
