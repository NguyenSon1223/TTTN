using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly IWebHostEnvironment _environment;

        public ProductController(ProductService productService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _environment = environment;
        }

        // ========== DANH SÁCH SẢN PHẨM ==========
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

        // ========== CHI TIẾT SẢN PHẨM ==========
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // ========== TẠO SẢN PHẨM ==========
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View(product);

            // ✅ Xử lý upload hình ảnh
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadDir = Path.Combine(_environment.WebRootPath, "img");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileExt = Path.GetExtension(ImageFile.FileName);
                var fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExt}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = uniqueFileName;
            }

            await _productService.CreateAsync(product);
            TempData["success"] = "✅ Thêm sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ========== CHỈNH SỬA SẢN PHẨM ==========
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var product = await _productService.GetByIdAsync(id);
            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Product updatedProduct, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
                return View(updatedProduct);

            var existingProduct = await _productService.GetByIdAsync(id);
            if (existingProduct == null)
                return NotFound();

            // ✅ Nếu có upload hình mới
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var uploadDir = Path.Combine(_environment.WebRootPath, "img");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileExt = Path.GetExtension(ImageFile.FileName);
                var fileName = Path.GetFileNameWithoutExtension(ImageFile.FileName);
                var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{fileExt}";
                var filePath = Path.Combine(uploadDir, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(fileStream);
                }

                // ✅ Xóa hình cũ nếu có
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                {
                    var oldImagePath = Path.Combine(uploadDir, existingProduct.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                updatedProduct.ImageUrl = uniqueFileName;
            }
            else
            {
                // Giữ lại hình cũ
                updatedProduct.ImageUrl = existingProduct.ImageUrl;
            }

            await _productService.UpdateAsync(id, updatedProduct);
            TempData["success"] = "✅ Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Index));
        }

        // ========== XOÁ SẢN PHẨM ==========
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            var product = await _productService.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            // Xóa hình nếu có
            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var uploadDir = Path.Combine(_environment.WebRootPath, "img");
                var oldImagePath = Path.Combine(uploadDir, product.ImageUrl);
                if (System.IO.File.Exists(oldImagePath))
                    System.IO.File.Delete(oldImagePath);
            }

            await _productService.DeleteAsync(id);
            return Ok(new { message = "Đã xóa sản phẩm!" });
        }

        // ========== THÊM VÀO GIỎ HÀNG ==========
        [HttpPost]
        public IActionResult AddToCart(string id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            // Logic tạm (sẽ nối với CartService sau)
            TempData["success"] = $"🛒 Đã thêm sản phẩm ID: {id} vào giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }
    }
}
