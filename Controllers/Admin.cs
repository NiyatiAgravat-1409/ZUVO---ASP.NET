using Microsoft.AspNetCore.Mvc;

namespace ZUVO_MVC_.Controllers
{
    public class Admin : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AdminDashboard()
        {
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
    }
}
