using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly CategoryService _categoryService;
        private readonly IWebHostEnvironment _environtment;

        public ProductController(ProductService productService, CategoryService categoryService, IWebHostEnvironment environment)
        {
            _productService = productService;
            _categoryService = categoryService;
            _environtment = environment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var products = _productService.GetAll();
            var categories = _categoryService.GetAll();

            var viewModel = products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.ImageUrl,
                p.Price,
                CategoryName = categories.FirstOrDefault(c => c.Id == p.CategoryId)?.Name ?? "Không xác định"
            }).ToList();

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Categories = _categoryService.GetAll();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var ext = Path.GetExtension(imageFile.FileName).ToLower();
                    var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

                    if (!allowedExt.Contains(ext))
                    {
                        TempData["Error"] = "Chỉ được tải lên file ảnh (jpg, png, gif, webp)!";
                        ViewBag.Categories = _categoryService.GetAll();
                        return View(product);
                    }

                    var uploadDir = Path.Combine(_environtment.WebRootPath, "img");
                    Directory.CreateDirectory(uploadDir);

                    var uniqueFileName = $"{Path.GetFileNameWithoutExtension(imageFile.FileName)}_{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadDir, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImageUrl = uniqueFileName;
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = _categoryService.GetAll();
                    TempData["Error"] = "Dữ liệu không hợp lệ!";
                    return View(product);
                }

                if (string.IsNullOrEmpty(product.ImageUrl))
                    product.ImageUrl = string.Empty;

                _productService.Create(product);
                TempData["Message"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Categories = _categoryService.GetAll();
                TempData["Error"] = $"Lỗi khi tạo sản phẩm: {ex.Message}";
                return View(product);
            }
        }



        [HttpGet]
        public IActionResult Edit(string id)
        {
            var product = _productService.GetById(id);
            if (product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = _categoryService.GetAll();
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(string id, Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = _categoryService.GetAll();
                    TempData["Error"] = "Dữ liệu không hợp lệ!";
                    return View(product);
                }

                _productService.Update(id, product);
                TempData["Message"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Categories = _categoryService.GetAll();
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
                return View(product);
            }
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            try
            {
                _productService.Delete(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
