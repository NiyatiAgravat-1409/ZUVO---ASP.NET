using Microsoft.AspNetCore.Mvc;

namespace ZUVO_MVC_.Controllers
{
    public class HostController : Controller
    {
        public IActionResult BecomeHost()
        {
            return View();
        }
    }
}