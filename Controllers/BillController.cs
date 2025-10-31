using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class BillController : Controller
    {
        private readonly BillService _billService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BillController(BillService billService, UserManager<ApplicationUser> userManager)
        {
            _billService = billService;
            _userManager = userManager;
        }

        // ✅ 1. Hiển thị tất cả hóa đơn đã thanh toán của user hiện tại
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Lấy ID user đang đăng nhập
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách hóa đơn từ database
            var bills = await _billService.GetBillsByUserIdAsync(userId);

            // Nếu người dùng chưa có hóa đơn
            if (bills == null || !bills.Any())
            {
                ViewBag.Message = "Bạn chưa có giao dịch nào.";
                return View("Empty");
            }

            // Trả danh sách hóa đơn cho View
            return View(bills.OrderByDescending(b => b.PaidAt).ToList());
        }

        // ✅ 2. Hiển thị chi tiết 1 hóa đơn cụ thể
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Index");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bill = await _billService.GetBillByIdAsync(id);

            // Kiểm tra hóa đơn có thuộc về user không
            if (bill == null || bill.UserId != userId)
            {
                TempData["Error"] = "Bạn không có quyền xem hóa đơn này.";
                return RedirectToAction("Index");
            }

            return View(bill);
        }
    }
}
