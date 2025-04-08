using Microsoft.AspNetCore.Mvc;

namespace ZUVO_MVC_.Controllers
{
    public class AfterLoginController : Controller
    {
        public IActionResult HomePAge()
        {
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
