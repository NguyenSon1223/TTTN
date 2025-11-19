using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
