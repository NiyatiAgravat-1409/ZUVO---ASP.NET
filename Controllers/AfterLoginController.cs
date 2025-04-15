using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZUVO_MVC_.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace ZUVO_MVC_.Controllers
{
    [Route("[controller]/[action]")]
    public class AfterLoginController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AfterLoginController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AfterLoginController(UserManager<Users> userManager, ILogger<AfterLoginController> logger, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> HomePage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                ViewData["FullName"] = user.FullName;
            }
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            _logger.LogInformation($"Loading profile for user: {user.UserName}");
            _logger.LogInformation($"Profile picture path: {user.ProfilePicPath}");

            var model = new Users
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserName = user.UserName,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                LicenseNo = user.LicenseNo,
                IssueState = user.IssueState,
                ExpiryDate = user.ExpiryDate,
                UploadPhoto = user.UploadPhoto,
                ProfilePicPath = user.ProfilePicPath
            };

            _logger.LogInformation($"Model profile picture path: {model.ProfilePicPath}");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] Users model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;
            user.UserName = model.UserName;
            user.Gender = model.Gender;
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address;
            user.LicenseNo = model.LicenseNo;
            user.IssueState = model.IssueState;
            user.ExpiryDate = model.ExpiryDate;
            user.UploadPhoto = model.UploadPhoto;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true });
            }
            else
            {
                _logger.LogError("Failed to update profile: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return Json(new { success = false, errors = result.Errors.Select(e => e.Description) });
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfilePic(IFormFile profilePic)
        {
            try
            {
                // Get user
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return NotFound("User not found");

                // Validate file
                if (profilePic == null || profilePic.Length == 0)
                    return BadRequest("No file uploaded");

                // Validate extension
                var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(profilePic.FileName).ToLower();
                if (!validExtensions.Contains(extension))
                    return BadRequest("Invalid file type");

                // Validate size (2MB max)
                if (profilePic.Length > 2 * 1024 * 1024)
                    return BadRequest("File too large (max 2MB)");

                // Create uploads folder if it doesn't exist
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads/profile-pics");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var fileName = $"{user.Id}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                _logger.LogInformation($"Saving file to: {filePath}");
                _logger.LogInformation($"WebRootPath: {_webHostEnvironment.WebRootPath}");

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profilePic.CopyToAsync(stream);
                }

                // Update user profile
                user.ProfilePicPath = $"/uploads/profile-pics/{fileName}";
                var result = await _userManager.UpdateAsync(user);
                
                if (!result.Succeeded)
                {
                    _logger.LogError($"Failed to update user profile: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return StatusCode(500, "Failed to update user profile");
                }

                _logger.LogInformation($"Updated user profile picture path to: {user.ProfilePicPath}");

                // Verify the update
                var updatedUser = await _userManager.FindByIdAsync(user.Id);
                _logger.LogInformation($"Verified user profile picture path: {updatedUser.ProfilePicPath}");

                return Ok(new { path = user.ProfilePicPath });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UploadProfilePic: {ex}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        public IActionResult Cars()
        {
            return View();
        }
    }
}
