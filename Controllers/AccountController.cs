using Microsoft.AspNetCore.Mvc;

namespace ZUVO_MVC_.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(string Email, string Password)
        {
            // Dummy authentication logic (replace with database logic)
            if (Email == "admin@example.com" && Password == "password123")
            {
                return RedirectToAction("Homepage", "AfterLogin");
            }

            ViewBag.Error = "Invalid email or password";
            return View();
        }
    }
}
