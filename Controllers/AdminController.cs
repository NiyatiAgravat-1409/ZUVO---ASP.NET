using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZUVO_MVC_.Data;
using ZUVO_MVC_.ViewModels;

namespace ZUVO_MVC_.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        public AdminController(AppDbContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(AdminLoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var admin = _context.Admins
                    .FirstOrDefault(a => a.Email == model.Email && a.Password == model.Password);

                if (admin != null)
                {
                    HttpContext.Session.SetString("AdminEmail", admin.Email);
                    return RedirectToAction("AdminDashboard", "Admin");
                }

                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
            }

            return View(model);
        }

        public IActionResult AdminDashboard()
        {
            var email = HttpContext.Session.GetString("AdminEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Index");

            ViewBag.Email = email;
            return View();
        }

        public IActionResult Bookings()
        {
            return View();  
        }

        public IActionResult Units()
        {
            return View();
        }

        public IActionResult Clients()
        {
            return View();
        }

        public IActionResult Payments()
        {
            return View();
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
