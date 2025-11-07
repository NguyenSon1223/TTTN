using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System;

namespace Ecommerce.Controllers
{
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var categories = _categoryService.GetAll();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            try
            {
                if (string.IsNullOrEmpty(category.Id))
                    category.Id = ObjectId.GenerateNewId().ToString();

                ModelState.Remove("Id");

                if (!ModelState.IsValid)
                {
                    foreach (var err in ModelState.Values)
                        foreach (var e in err.Errors)
                            Console.WriteLine($"Model Error: {e.ErrorMessage}");

                    TempData["Error"] = "Dữ liệu không hợp lệ! Vui lòng kiểm tra lại.";
                    return View(category);
                }

                _categoryService.Create(category);
                TempData["Message"] = "Thêm danh mục thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi tạo danh mục: " + ex.Message;
                return View(category);
            }
        }

        [HttpGet]
        public IActionResult Edit(string id)
        {
            var category = _categoryService.GetById(id);
            if (category == null)
            {
                TempData["Error"] = "Không tìm thấy danh mục.";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(string id, Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Dữ liệu không hợp lệ!";
                    return View(category);
                }

                _categoryService.Update(id, category);
                TempData["Message"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi cập nhật: " + ex.Message;
                return View(category);
            }
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            try
            {
                _categoryService.Delete(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
