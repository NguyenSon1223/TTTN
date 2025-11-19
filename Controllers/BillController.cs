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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử thanh toán.";
                return RedirectToAction("Login", "Account");
            }

            var bills = await _billService.GetBillsByUserIdAsync(userId);

            if (bills == null || !bills.Any())
            {
                ViewBag.Message = "Bạn chưa có giao dịch nào.";
                return View();
            }

            return View(bills.OrderByDescending(b => b.PaidAt).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Index");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bill = await _billService.GetBillByIdAsync(id);

            if (bill == null || bill.UserId != userId)
            {
                TempData["Error"] = "Bạn không có quyền xem hóa đơn này.";
                return RedirectToAction("Index");
            }

            return View(bill);
        }
    }
}
