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

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpGet]
        public IActionResult Listyourcar()
        {
            return View();
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Listyourcar(CarViewModel model)
        {
            _logger.LogInformation("Starting Listyourcar action");
            
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed. Errors: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            try
            {
                var hostId = User.FindFirst("HostId")?.Value;
                if (string.IsNullOrEmpty(hostId))
                {
                    _logger.LogWarning("HostId not found in claims");
                    return RedirectToAction("HostSignin");
                }

                _logger.LogInformation("Creating new car for host: {HostId}", hostId);

                // Create new car
                var car = new Car
                {
                    HostId = hostId,
                    Make = model.Make,
                    Model = model.Model,
                    Year = model.Year,
                    Color = model.Color,
                    LicensePlateNo = model.LicensePlateNo,
                    VIN = model.VIN,
                    Transmission = model.Transmission,
                    FuelType = model.FuelType,
                    Seats = model.Seats,
                    DailyRate = model.DailyRate,
                    Description = model.Description,
                    AllowCancellation = model.AllowCancellation,
                    MinRentalPeriod = model.MinRentalPeriod,
                    Mileage = model.Mileage,
                    AdditionalFeatures = model.AdditionalFeatures,
                    InsuranceType = model.InsuranceType,
                    InsuranceNumber = model.InsuranceNumber,
                    InsuranceCompany = model.InsuranceCompany,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow
                };

                _logger.LogInformation("Adding car to context: {@Car}", car);
                _context.Cars.Add(car);

                // Save car first to get the CarId
                _logger.LogInformation("Saving car to database");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Car saved successfully with ID: {CarId}", car.CarId);

                // Handle document uploads
                if (model.CarRegistrationDocument != null && model.CarRegistrationDocument.Length > 0)
                {
                    _logger.LogInformation("Processing car registration document");
                    var docFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.CarRegistrationDocument.FileName)}";
                    var docFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "documents", docFileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(docFilePath));
                    
                    using (var stream = new FileStream(docFilePath, FileMode.Create))
                    {
                        await model.CarRegistrationDocument.CopyToAsync(stream);
                    }
                    
                    car.CarRegistrationDocumentPath = $"/uploads/documents/{docFileName}";
                    _logger.LogInformation("Car registration document saved: {Path}", car.CarRegistrationDocumentPath);
                }

                if (model.InsuranceCertificate != null && model.InsuranceCertificate.Length > 0)
                {
                    _logger.LogInformation("Processing insurance certificate");
                    var certFileName = $"{Guid.NewGuid()}{Path.GetExtension(model.InsuranceCertificate.FileName)}";
                    var certFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "documents", certFileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(certFilePath));
                    
                    using (var stream = new FileStream(certFilePath, FileMode.Create))
                    {
                        await model.InsuranceCertificate.CopyToAsync(stream);
                    }
                    
                    car.InsuranceCertificatePath = $"/uploads/documents/{certFileName}";
                    _logger.LogInformation("Insurance certificate saved: {Path}", car.InsuranceCertificatePath);
                }

                // Link uploaded photos to the car
                if (!string.IsNullOrEmpty(model.Photos))
                {
                    _logger.LogInformation("Processing photos: {Photos}", model.Photos);
                    try
                    {
                        var photoIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(model.Photos);
                        if (photoIds != null && photoIds.Count > 0)
                        {
                            _logger.LogInformation("Found {Count} photos to link", photoIds.Count);
                            var photos = await _context.CarPhotos
                                .Where(p => photoIds.Contains(p.PhotoId))
                                .ToListAsync();

                            foreach (var photo in photos)
                            {
                                photo.CarId = car.CarId;
                                photo.IsPrimary = photos.IndexOf(photo) == 0;
                                _logger.LogInformation("Linked photo {PhotoId} to car {CarId}", photo.PhotoId, car.CarId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing photos: {Photos}", model.Photos);
                    }
                }

                _logger.LogInformation("Saving final changes to database");
                await _context.SaveChangesAsync();
                _logger.LogInformation("All changes saved successfully");

                return RedirectToAction("HostDashboard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing car. Model: {@Model}", model);
                ModelState.AddModelError("", "An error occurred while listing your car. Please try again.");
                return View(model);
            }
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
        public async Task<IActionResult> UploadPhoto(IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                {
                    return BadRequest("No photo uploaded");
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars", fileName);

                // Ensure directory exists
                Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                // Create temporary photo record
                var carPhoto = new CarPhoto
                {
                    PhotoId = Guid.NewGuid().ToString(),
                    FileName = fileName,
                    FilePath = $"/uploads/cars/{fileName}",
                    IsPrimary = false,
                    UploadedAt = DateTime.UtcNow
                };

                _context.CarPhotos.Add(carPhoto);
                await _context.SaveChangesAsync();

                return Json(new { photoId = carPhoto.PhotoId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading photo");
                return StatusCode(500, "Error uploading photo");
            }
        }

        [Authorize(AuthenticationSchemes = "HostAuth")]
        [HttpDelete]
        public async Task<IActionResult> DeletePhoto(string id)
        {
            try
            {
                var photo = await _context.CarPhotos.FindAsync(id);
                if (photo == null)
                {
                    return NotFound();
                }

                // Delete file from storage
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "cars", photo.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Remove from database
                _context.CarPhotos.Remove(photo);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo");
                return StatusCode(500, "Error deleting photo");
            }
        }
    }
}

