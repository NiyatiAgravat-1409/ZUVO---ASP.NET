using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;

namespace ZUVO_MVC_.Models
{
    public class HostUser
    {
        [Key]
        public string HostId { get; set; }

        // Personal Information
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email Address")]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [Display(Name =  "Gender")]
        public Gender? Gender { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DOB { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [Display(Name = "Address")]
        public string Address { get; set; }

        // Driving License Details
        [Required(ErrorMessage = "License number is required")]
        [Display(Name = "License Number")]
        public string LicenseNumber { get; set; }

        [Required(ErrorMessage = "Issue state is required")]
        [Display(Name = "Issue State")]
        public string IssueState { get; set; }

        [Display(Name = "License Expiry Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "License Photo")]
        public string? LicensePhoto { get; set; }

        // Profile Picture
        [Display(Name = "Profile Picture Path")]
        public string? ProfilePicturePath { get; set; }

        // Payment & Billing
        [Display(Name = "Wallet Linked")]
        public bool WalletLinked { get; set; } = false;

        [Display(Name = "Wallet Balance")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalance { get; set; } = 0;

        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        // Navigation Property for Transactions
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Navigation property for Cars
        public virtual ICollection<Car> Cars { get; set; }

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Password methods
        public void SetPassword(string password)
        {
            // Generate a random salt
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            PasswordSalt = Convert.ToBase64String(saltBytes);

            // Hash the password with the salt
            PasswordHash = HashPassword(password, PasswordSalt);
        }

        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(PasswordHash) || string.IsNullOrEmpty(PasswordSalt))
                return false;

            string hashedPassword = HashPassword(password, PasswordSalt);
            return hashedPassword == PasswordHash;
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }

    public enum Gender
    {
        [Display(Name = "Male")]
        Male,
        [Display(Name = "Female")]
        Female,
        [Display(Name = "Other")]
        Other
    }

    public class Transaction
    {
        [Key]
        public string TransactionId { get; set; }

        [Required]
        public string HostId { get; set; }

        [ForeignKey("HostId")]
        public virtual HostUser HostUser { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime StartDate { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Amount")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Method")]
        public string Method { get; set; }

        [Display(Name = "Invoice URL")]
        [Url(ErrorMessage = "Invalid URL")]
        public string? InvoiceUrl { get; set; }

        [Display(Name = "Vehicle Image")]
        public string? VehicleImage { get; set; }

        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
} 