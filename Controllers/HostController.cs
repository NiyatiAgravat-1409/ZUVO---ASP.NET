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
using Microsoft.Data.SqlClient;
using System.Text.RegularExpressions;
using Polly;  // Add this for retry policy

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
                    Username = model.Email.Split('@')[0], // just to set something by default
                    CreatedAt = DateTime.UtcNow
                };

                // Set the password with hashing
                newUser.SetPassword(model.Password);

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
            if (!ModelState.IsValid) return View(model);

            // 1. First verify basic email format
            if (string.IsNullOrWhiteSpace(model.Email) || !model.Email.Contains("@"))
            {
                ModelState.AddModelError(string.Empty, "Invalid email format");
                return View(model);
            }

            // 2. Check database availability
            try
            {
                if (!await TestDatabaseConnection())
                {
                    ModelState.AddModelError(string.Empty, "System temporarily unavailable. Please try again later.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                ModelState.AddModelError(string.Empty, "System temporarily unavailable");
                return View(model);
            }

            // 3. Main login logic with retry policy
            try
            {
                var policy = Policy
                    .Handle<SqlException>()
                    .WaitAndRetryAsync(
                        retryCount: 2,
                        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                        onRetry: (exception, delay, attempt, context) =>
                        {
                            _logger.LogWarning($"Retry {attempt} due to {exception.Message}");
                        });

                var result = await policy.ExecuteAsync(async () =>
                {
                    return await AttemptLogin(model);
                });

                if (result is not null)
                {
                    return result;
                }

                ModelState.AddModelError(string.Empty, "Invalid email or password");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login process failed");
                ModelState.AddModelError(string.Empty, "System temporarily unavailable");
                return View(model);
            }
        }

        private async Task<bool> TestDatabaseConnection()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        private async Task<IActionResult> AttemptLogin(HostLoginViewModel model)
        {
            var user = await _context.HostUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !user.VerifyPassword(model.Password))
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("HostId", user.HostId),
                new Claim(ClaimTypes.Role, "Host")
            };

            var identity = new ClaimsIdentity(claims, "HostAuth");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                "HostAuth",
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                });

            return RedirectToAction("HostDashboard", "Host");
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

            // Fetch the host's cars with their primary photos
            var hostCars = await _context.Cars
                .Where(c => c.HostId == hostId)
                .OrderByDescending(c => c.CreatedAt)
                .Include(c => c.Photos)
                .ToListAsync();

            // Pass the host's full name and cars to the view
            ViewBag.HostName = hostUser.Name;
            return View(hostCars);
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
            var hostId = User.FindFirst("HostId")?.Value;
            if (string.IsNullOrEmpty(hostId))
            {
                _logger.LogWarning("HostId claim not found in user claims");
                return RedirectToAction("HostSignin", "Host");
            }

            var carTypes = _context.CarTypes.ToList();
            ViewBag.CarTypes = carTypes;

            var viewModel = new CarViewModel
            {
                HostId = hostId
            };
            
            return View(viewModel);
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Listyourcar(CarViewModel viewModel)
        {
            _logger.LogInformation("Starting car listing submission for HostId: {HostId}", viewModel.HostId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for car listing. Errors: {@Errors}", 
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                
                ViewBag.CarTypes = _context.CarTypes.ToList();
                return View(viewModel);
            }

            try
            {
                // Generate new CarId
                var carId = Guid.NewGuid().ToString();
                _logger.LogInformation("Generated new CarId: {CarId}", carId);

                // Create the car record
                var car = new Car
                {
                    CarId = carId,
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
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Cars.Add(car);

                // Handle car photos
                if (viewModel.CarPhotos != null && viewModel.CarPhotos.Count > 0)
                {
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars", carId);
                    Directory.CreateDirectory(uploadsFolder);

                    for (int i = 0; i < viewModel.CarPhotos.Count; i++)
                    {
                        var photo = viewModel.CarPhotos[i];
                        if (photo != null && photo.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);
                            var relativePath = Path.Combine("uploads", "cars", carId, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await photo.CopyToAsync(stream);
                            }

                            var carPhoto = new CarPhoto
                            {
                                PhotoId = Guid.NewGuid().ToString(),
                                CarId = carId,
                                FileName = photo.FileName,
                                FilePath = relativePath,
                                IsPrimary = i == 0, // First photo is primary
                                UploadedAt = DateTime.UtcNow
                            };

                            _context.CarPhotos.Add(carPhoto);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Car successfully listed with ID: {CarId}", car.CarId);
                TempData["Success"] = "Car listed successfully!";
                return RedirectToAction("HostDashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while listing car for HostId: {HostId}", viewModel.HostId);
                ModelState.AddModelError("", "An error occurred while saving your car details. Please try again.");
                ViewBag.CarTypes = _context.CarTypes.ToList();
                return View(viewModel);
            }
        }

        private async Task<string> SaveFile(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", fileName);
            
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            
            return Path.Combine("uploads", fileName);
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        public async Task<IActionResult> HostProfile()
        {
            var hostId = User.FindFirst("HostId")?.Value;
            if (string.IsNullOrEmpty(hostId))
            {
                return RedirectToAction("HostSignin");
            }

            var hostUser = await _context.HostUsers.FindAsync(hostId);
            if (hostUser == null)
            {
                return NotFound();
            }

            var viewModel = new HostProfileViewModel
            {
                HostId = hostUser.HostId,
                Email = hostUser.Email,
                Name = hostUser.Name,
                Mobile = hostUser.Mobile,
                Username = hostUser.Username,
                Gender = hostUser.Gender?.ToString(),
                DOB = hostUser.DOB,
                Address = hostUser.Address,
                LicenseNumber = hostUser.LicenseNumber,
                IssueState = hostUser.IssueState,
                ExpiryDate = hostUser.ExpiryDate
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "HostAuth")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([FromBody] HostProfileViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, errors = errors });
                }

                var hostUser = await _context.HostUsers.FindAsync(model.HostId);
                if (hostUser == null)
                {
                    return NotFound(new { success = false, message = "Host not found." });
                }

                // Update the host user properties
                hostUser.Name = model.Name;
                hostUser.Mobile = model.Mobile;
                hostUser.Username = model.Username;
                hostUser.Gender = Enum.TryParse<Gender>(model.Gender, out var gender) ? gender : null;
                hostUser.DOB = model.DOB;
                hostUser.Address = model.Address;
                hostUser.LicenseNumber = model.LicenseNumber;
                hostUser.IssueState = model.IssueState;
                hostUser.ExpiryDate = model.ExpiryDate;
                hostUser.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(model.ProfilePicturePath))
                {
                    hostUser.ProfilePicturePath = model.ProfilePicturePath;
                }

                _context.Update(hostUser);
                await _context.SaveChangesAsync();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating host profile");
                return Json(new { success = false, message = "An error occurred while updating the profile." });
            }
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "HostAuth")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
        {
            if (profilePicture == null || profilePicture.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file uploaded" });
            }

            var hostId = User.FindFirst("HostId")?.Value;
            if (string.IsNullOrEmpty(hostId))
            {
                return Unauthorized(new { success = false, message = "User not authenticated" });
            }

            var hostUser = await _context.HostUsers.FindAsync(hostId);
            if (hostUser == null)
            {
                return NotFound(new { success = false, message = "Host not found" });
            }

            try
            {
                var fileName = $"{hostId}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(profilePicture.FileName)}";
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "hosts");
                Directory.CreateDirectory(uploadsFolder);

                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePicture.CopyToAsync(fileStream);
                }

                // Update the profile picture path in the database
                hostUser.ProfilePicturePath = $"/uploads/hosts/{fileName}";
                _context.Update(hostUser);
                await _context.SaveChangesAsync();

                return Json(new { success = true, filePath = hostUser.ProfilePicturePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading profile picture");
                return StatusCode(500, new { success = false, message = "Error uploading profile picture" });
            }
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

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadCarPhotos(List<IFormFile> carPhotos)
        {
            if (carPhotos == null || carPhotos.Count == 0)
            {
                return Json(new { success = false, message = "Please select at least one photo." });
            }

            var uploadedPaths = new List<string>();

            foreach (var photo in carPhotos)
            {
                try
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await photo.CopyToAsync(fileStream);
                    }

                    var relativePath = Path.Combine("uploads", fileName);
                    uploadedPaths.Add(relativePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading photo");
                    return Json(new { success = false, message = "Error uploading photos. Please try again." });
                }
            }

            return Json(new { success = true, photoPaths = uploadedPaths });
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "HostAuth")]
        public async Task<IActionResult> DeleteCar(string id)
        {
            try
            {
                var hostId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(hostId))
                {
                    _logger.LogWarning("Host ID not found in claims");
                    return Unauthorized();
                }

                var car = await _context.Cars
                    .FirstOrDefaultAsync(c => c.CarId == id && c.HostId == hostId);

                if (car == null)
                {
                    _logger.LogWarning($"Car with ID {id} not found for host {hostId}");
                    return NotFound();
                }

                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Car {id} deleted successfully by host {hostId}");
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting car {id}");
                return StatusCode(500, "An error occurred while deleting the car");
            }
        }
    }
}

