using System.ComponentModel.DataAnnotations;

namespace ZUVO_MVC_.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required. ")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required. ")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Address is required. ")]
        [StringLength(500, ErrorMessage = "The address must not exceed 500 characters.")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Date of Birth is required. ")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public string DateOfBirth { get; set; }
        [Required(ErrorMessage = "Password is required. ")]
        [StringLength(40, MinimumLength = 8, ErrorMessage ="The {0} must be at {2} and at max {1} character")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Confirm Password is required. ")]
        public string ConfirmPassword { get; set; }
    }
}
