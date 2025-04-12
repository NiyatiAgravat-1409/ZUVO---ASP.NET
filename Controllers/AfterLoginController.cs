using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZUVO_MVC_.Models;

namespace ZUVO_MVC_.Controllers
{
    public class AfterLoginController : Controller
    {
        private readonly UserManager<Users> _userManager;

        public AfterLoginController(UserManager<Users> userManager)
        {
            _userManager = userManager;
        }

    public async Task<IActionResult> HomePAge()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewData["FullName"] = user.FullName;
            }
            return View();
        }
        
        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult Cars()
        {
            return View();
        }

    }
}
