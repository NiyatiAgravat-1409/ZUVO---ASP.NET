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
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

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

                // Redirect to signin page
                return RedirectToAction("HostSignin", "Host");
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

                // Verify password directly since HostUser doesn't use Identity
                if (string.Equals(hostUser.Password, model.Password, StringComparison.OrdinalIgnoreCase))
                {
                    // Create claims for the host user
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, hostUser.Name),
                        new Claim(ClaimTypes.Email, hostUser.Email),
                        new Claim("HostId", hostUser.HostId),
                        new Claim(ClaimTypes.Role, "Host")
                    };

                    // Create claims identity and principal
                    var claimsIdentity = new ClaimsIdentity(claims, "HostAuth");
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                    // Sign in the user
                    await HttpContext.SignInAsync("HostAuth", claimsPrincipal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                    });

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
            var carTypes = _context.CarTypes.ToList();
            return View(carTypes);
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpGet]
        public IActionResult Listyourcar()
        {
            var carTypes = _context.CarTypes.ToList(); // Make sure _context is your DB context and it's properly injected
            ViewBag.CarTypes = carTypes;

            var viewModel = new CarViewModel();
            ViewData["HostId"] = new SelectList(_context.HostUsers, "HostId", "HostId");
            return View(viewModel);
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Listyourcar(CarViewModel viewModel)
        {
            // Log HostId
            Console.WriteLine("Host ID: " + viewModel.HostId);

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is NOT valid. Errors:");

                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Field: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }

                // Repopulate view data
                ViewBag.CarTypes = _context.CarTypes.ToList();
                ViewData["HostId"] = new SelectList(_context.HostUsers, "HostId", "HostId", viewModel.HostId);
                return View(viewModel);
            }

            string registrationDocPath = null;

            if (viewModel.CarRegistrationDocument != null)
            {
                try
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(viewModel.CarRegistrationDocument.FileName);
                    var path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await viewModel.CarRegistrationDocument.CopyToAsync(stream);
                    }

                    registrationDocPath = Path.Combine("uploads", fileName);
                    Console.WriteLine("File uploaded to: " + registrationDocPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("File upload failed: " + ex.Message);
                }
            }
            else
            {
                Console.WriteLine("No file uploaded.");
            }

            // Log before saving
            Console.WriteLine("Creating car object...");

            var car = new Car
            {
                CarId = Guid.NewGuid().ToString(),
                HostId = viewModel.HostId,
                Make = viewModel.Make,
                Model = viewModel.Model,
                Year = viewModel.Year,
                Color = viewModel.Color,
                LicensePlateNo = viewModel.LicensePlateNo,
                VIN = viewModel.VIN,
                Transmission = viewModel.Transmission,
                FuelType = viewModel.FuelType,
                Seats = viewModel.Seats,
                DailyRate = viewModel.DailyRate,
                Description = viewModel.Description,
                AllowCancellation = viewModel.AllowCancellation,
                MinRentalPeriod = viewModel.MinRentalPeriod,
                Mileage = viewModel.Mileage,
                AdditionalFeatures = viewModel.AdditionalFeatures,
                InsuranceType = viewModel.InsuranceType,
                InsuranceNumber = viewModel.InsuranceNumber,
                InsuranceCompany = viewModel.InsuranceCompany,
                CarRegistrationDocumentPath = registrationDocPath,
                InsuranceCertificatePath = null,
                IsAvailable = viewModel.IsAvailable,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Cars.Add(car);
            await _context.SaveChangesAsync();

            Console.WriteLine("Car saved successfully with ID: " + car.CarId);

            return RedirectToAction("HostDashboard");
        }
        public IActionResult HostProfile()
        {
            return View();
        }

        public IActionResult EditCarDetails()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "File is empty" });

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Json(new { success = true, fileName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCarPhotos(string carId, List<IFormFile> carPhotos)
        {
            if (carPhotos == null || carPhotos.Count == 0)
            {
                ModelState.AddModelError("carPhotos", "Please select at least one photo.");
                return View(); // Return to your page if no photos were uploaded.
            }

            var car = await _context.Cars.FindAsync(carId);
            if (car == null)
            {
                return NotFound();
            }

            foreach (var photo in carPhotos)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(fileStream);
                }

                var carPhoto = new CarPhoto
                {
                    CarId = carId,
                    FileName = fileName,
                    FilePath = Path.Combine("uploads", fileName),
                    IsPrimary = false, // Set primary to false by default
                    UploadedAt = DateTime.UtcNow
                };

                // Add photo to database
                _context.CarPhotos.Add(carPhoto);
            }       

            // If there is no photo already marked as primary, mark the first uploaded photo as primary
            var primaryPhoto = await _context.CarPhotos.FirstOrDefaultAsync(p => p.CarId == carId && p.IsPrimary);
            if (primaryPhoto == null && carPhotos.Count > 0)
            {
                var firstUploadedPhoto = await _context.CarPhotos.FirstAsync(p => p.CarId == carId);
                firstUploadedPhoto.IsPrimary = true;
                _context.CarPhotos.Update(firstUploadedPhoto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("CarDetails", new { id = carId });
        }
    }
}

