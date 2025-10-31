using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(CartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        // ✅ Hiển thị giỏ hàng
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            // Ép kiểu GUID sang string
            var cart = await _cartService.GetCartByUserIdAsync(user.Id.ToString());
            return View(cart);
        }

        // ✅ Thêm sản phẩm vào giỏ
        [HttpPost]
        public async Task<IActionResult> Add(string productId, int qty = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _cartService.AddToCartAsync(user.Id.ToString(), productId, qty);
            TempData["Success"] = "✅ Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index", "Product");
        }

        // ✅ Cập nhật số lượng sản phẩm trong giỏ
        [HttpPost]
        public async Task<IActionResult> Update(string productId, int qty)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _cartService.UpdateQuantityAsync(user.Id.ToString(), productId, qty);
            return RedirectToAction("Index");
        }

        // ✅ Xóa sản phẩm khỏi giỏ
        [HttpPost]
        public async Task<IActionResult> Remove(string productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _cartService.RemoveFromCartAsync(user.Id.ToString(), productId);
            return RedirectToAction("Index");
        }

        // ✅ Xóa toàn bộ giỏ hàng
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _cartService.ClearCartAsync(user.Id.ToString());
            return RedirectToAction("Index");
        }
    }
}
