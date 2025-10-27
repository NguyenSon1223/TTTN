using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string productId, int qty = 1)
        {
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            await _cartService.AddToCartAsync(userId, productId, qty);
            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng!";
            return RedirectToAction("Index", "Product");
        }

        public async Task<IActionResult> Remove(string productId)
        {
            var userId = User.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
                await _cartService.RemoveFromCartAsync(userId, productId);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            var userId = User.Identity?.Name;
            if (!string.IsNullOrEmpty(userId))
                await _cartService.ClearCartAsync(userId);

            return RedirectToAction("Index");
        }
    }
}
