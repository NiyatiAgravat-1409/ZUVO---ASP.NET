using Microsoft.AspNetCore.Mvc;

namespace ZUVO_MVC_.Controllers
{
    public class HostController : Controller
    {
        public IActionResult BecomeHost()
        {
            return View();
        }

        public IActionResult HostDashboard()
        {
            return View(); // Ensure this line is returning the correct view
        }

        public IActionResult Listyourcar()
        {
            return View();
        }

        public IActionResult HostProfile()
        {
            return View();
        }

        public IActionResult EditCarDetails()
        {
            return View();
        }

        public IActionResult HostSignin()
        {
            return View();
        }
    }


}
