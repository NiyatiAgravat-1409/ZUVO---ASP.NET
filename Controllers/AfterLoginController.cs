using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ZUVO_MVC_.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ZUVO_MVC_.Controllers
{
    public class AfterLoginController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<AfterLoginController> _logger;

        public AfterLoginController(UserManager<Users> userManager, ILogger<AfterLoginController> logger)
        {
            _userManager = userManager;
            _logger = logger;
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
                UploadPhoto = user.UploadPhoto
            };

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

        public IActionResult Cars()
        {
            return View();
        }
    }
}
