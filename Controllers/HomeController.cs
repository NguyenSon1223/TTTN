using Ecommerce.Models;
using Ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Ecommerce.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ProductService _productService;

    public HomeController(ILogger<HomeController> logger,  ProductService productService)
    {
        _logger = logger;
        _productService = productService;
        
    }

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

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
