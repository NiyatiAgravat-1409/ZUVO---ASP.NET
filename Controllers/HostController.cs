using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using System.Text.Encodings.Web;
using ZUVO_MVC_.Models;
using ZUVO_MVC_.ViewModels;
using Microsoft.EntityFrameworkCore;
using ZUVO_MVC_.Data;
using Microsoft.AspNetCore.Authentication;

namespace ZUVO_MVC_.Controllers
{
    public class HostController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly ILogger<HostController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public HostController(
            UserManager<Users> userManager,
            SignInManager<Users> signInManager,
            ILogger<HostController> logger,
            IWebHostEnvironment webHostEnvironment,
            RoleManager<IdentityRole> roleManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        public IActionResult HostSignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> HostSignUp(HostRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email already exists
                var existing = _context.HostUsers.FirstOrDefault(u => u.Email == model.Email);
                if (existing != null)
                {
                    ModelState.AddModelError("Email", "Email already registered.");
                    return View(model);
                }

                // Create a new HostUser
                var newUser = new HostUser
                {
                    HostId = Guid.NewGuid().ToString(),
                    Name = model.FullName,
                    Email = model.Email,
                    Password = model.Password, // 🔐 NOTE: Hash in production!
                    Username = model.Email.Split('@')[0], // just to set something by default
                    CreatedAt = DateTime.UtcNow
                };

                _context.HostUsers.Add(newUser);
                await _context.SaveChangesAsync();

                // Redirect or show success
                return RedirectToAction("Host", "HostSignin");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult HostSignin()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HostSignin(HostLoginViewModel model)
        {
            _logger.LogInformation("HostSignin attempt for email: {Email}", model.Email);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for email: {Email}", model.Email);
                return View(model);
            }

            try
            {
                // Check if the host exists in HostUsers table
                var hostUser = await _context.HostUsers.FirstOrDefaultAsync(h => h.Email == model.Email);
                
                if (hostUser == null)
                {
                    _logger.LogWarning("Host not found for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                _logger.LogInformation("Host found in database for email: {Email}", model.Email);

                // Verify password against HostUser table
                if (hostUser.Password == model.Password)
                {
                    _logger.LogInformation("Password verified for email: {Email}", model.Email);
                    
                    // Create claims for the host
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, hostUser.Name),
                        new Claim(ClaimTypes.Email, hostUser.Email),
                        new Claim("HostId", hostUser.HostId),
                        new Claim(ClaimTypes.Role, "Host")
                    };

                    // Create identity
                    var identity = new ClaimsIdentity(claims, "HostAuth");
                    var principal = new ClaimsPrincipal(identity);

                    // Sign in the host
                    await HttpContext.SignInAsync("HostAuth", principal);
                    
                    _logger.LogInformation("Host logged in successfully for email: {Email}", model.Email);
                    return RedirectToAction("HostDashboard", "Host");
                }
                else
                {
                    _logger.LogWarning("Invalid password for email: {Email}", model.Email);
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during host signin for email: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToAction("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action(
                    "ResetPassword",
                    "Host",
                    new { code = code },
                    protocol: Request.Scheme);

                // TODO: Implement email sending logic here

                return RedirectToAction("ForgotPasswordConfirmation");
            }

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string code = null)
        {
            if (code == null)
            {
                return BadRequest("A code must be supplied for password reset.");
            }
            else
            {
                var model = new HostResetPasswordViewModel { Code = code };
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(HostResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        public async Task<IActionResult> HostDashboard()
        {
            var hostId = User.FindFirst("HostId")?.Value;
            if (string.IsNullOrEmpty(hostId))
            {
                _logger.LogWarning("HostId claim not found in user claims");
                return RedirectToAction("HostSignin", "Host");
            }

            var hostUser = await _context.HostUsers.FindAsync(hostId);
            if (hostUser == null)
            {
                _logger.LogWarning("Host not found in database for HostId: {HostId}", hostId);
                return RedirectToAction("HostSignin", "Host");
            }

            // Pass the host's full name to the view
            ViewBag.HostName = hostUser.Name;
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Host logged out.");
            return RedirectToAction("Index", "Home");
        }

        public IActionResult BecomeHost()
        {
            return View();
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
    }
}

