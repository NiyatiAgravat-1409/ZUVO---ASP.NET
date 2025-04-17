    using System.ComponentModel.DataAnnotations;

namespace ZUVO_MVC_.ViewModels
{
    public class HostLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

}