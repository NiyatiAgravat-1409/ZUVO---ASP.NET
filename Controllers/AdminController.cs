using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ZUVO_MVC_.Models;
using ZUVO_MVC_.Data;
using ZUVO_MVC_.ViewModels;
using Microsoft.Extensions.Logging;

namespace ZUVO_MVC_.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(AppDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
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

            var cars = _context.Cars
                .Include(c => c.Host)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(cars); 
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

        public IActionResult Create()
        {
            var carTypes = _context.CarTypes.ToList();

            // Pass car types to the view (dropdown)
            ViewBag.CarTypes = carTypes;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(CarType carType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.CarTypes.Add(carType);
                    _context.SaveChanges();
                    TempData["Success"] = "Car type added successfully!";
                    return RedirectToAction("AdminDashboard");
                }
                catch (Exception ex)
                {
                    TempData["Error"] = "An error occurred while adding the car type. Please try again.";
                    _logger.LogError(ex, "Error adding car type");
                }
            }
            else
            {
                TempData["Error"] = "Please correct the errors in the form.";
            }

            return RedirectToAction("AdminDashboard");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCarAvailability(string id)
        {
            try
            {
                var car = await _context.Cars.FindAsync(id);
                if (car == null)
                {
                    return NotFound();
                }

                car.IsAvailable = !car.IsAvailable;
                await _context.SaveChangesAsync();

                return Json(new { isAvailable = car.IsAvailable });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling car availability");
                return StatusCode(500, "An error occurred while updating availability");
            }
        }

    }
}
