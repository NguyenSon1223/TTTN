using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Controllers
{
    public class OperationController : Controller
    {
        private UserManager<ApplicationUser> userManager;

        public OperationController (UserManager<ApplicationUser> userManager)
        {
            this.userManager = userManager;
        }
        public ViewResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create ( User user )
        {
            if (ModelState.IsValid)
            {
                ApplicationUser appUser = new ApplicationUser
                {
                    UserName = user.Name,
                    Email = user.Email,
                };
                IdentityResult result = await userManager.CreateAsync(appUser, user.Password);
                if (result.Succeeded)
                {
                    ViewBag.Message = "User Created Successfully !";
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
                
            }
            return View(user);
        }
    }
}
