using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ZUVO_MVC_.Models
{
    public class SignIn : Controller
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnPost()
        {
            // Fake login logic for demo
            if (Email == "test@example.com" && Password == "password123")
            {
                return RedirectToPage("HomePAge", "AfterLogin"); // Redirect to home page after successful login
            }
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View();
        }
    }
}
