using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ZUVO_MVC_.Models
{
    public class Users: IdentityUser 
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; }
        
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? LicenseNo { get; set; }
        public string? IssueState { get; set; }
        public string? ExpiryDate { get; set; }
        public string? UploadPhoto { get; set; }
        public string? ProfilePicPath { get; set; }
    }
}
