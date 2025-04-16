using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [Display(Name = "Mobile Number")]
        public string? Mobile { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Display(Name = "Gender")]
        public Gender? Gender { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DOB { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        // Driving License Details
        [Display(Name = "License Number")]
        public string? LicenseNumber { get; set; }

        [Display(Name = "Issue State")]
        public string? IssueState { get; set; }

        [Display(Name = "License Expiry Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? ExpiryDate { get; set; }

        [Display(Name = "License Photo")]
        public string? LicensePhoto { get; set; }

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

        // Timestamps
        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }
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