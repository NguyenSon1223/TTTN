using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        // =========================
        // 🟩 Hiển thị danh sách sản phẩm
        // =========================
        //public async Task<IActionResult> Index(string? category)
        //{
        //    var products = string.IsNullOrEmpty(category)
        //        ? await _productService.GetAllAsync()
        //        : await _productService.GetByCategoryAsync(category);

        //    return View(products);
        //}

        public async Task<IActionResult> Index(string? category)
        {
            var products = string.IsNullOrEmpty(category)
                ? await _productService.GetAllAsync()
                : await _productService.GetByCategoryAsync(category);

            // Lấy danh sách Category duy nhất
            var allProducts = await _productService.GetAllAsync();
            var categories = allProducts.Select(p => p.Category).Distinct().ToList();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            return View(products);
        }

        // =========================
        // 🟦 Hiển thị chi tiết sản phẩm
        // =========================
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // =========================
        // 🟨 Hiển thị form tạo mới
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Xử lý thêm sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            await _productService.CreateAsync(product);
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // 🟧 Hiển thị form chỉnh sửa
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        // Xử lý cập nhật sản phẩm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product updatedProduct)
        {
            if (!ModelState.IsValid)
                return View(updatedProduct);

            await _productService.UpdateAsync(id, updatedProduct);
            return RedirectToAction(nameof(Index));
        }

        // =========================
        // 🟥 Xóa sản phẩm
        // =========================
        //[HttpGet]
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (string.IsNullOrEmpty(id)) return NotFound();

        //    var product = await _productService.GetByIdAsync(id);
        //    if (product == null) return NotFound();

        //    return View(product);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    await _productService.DeleteAsync(id);
        //    return RedirectToAction(nameof(Index));
        //}

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            await _productService.DeleteAsync(id);
            return Ok(); // Trả về HTTP 200 cho fetch()
        }
    }
}
