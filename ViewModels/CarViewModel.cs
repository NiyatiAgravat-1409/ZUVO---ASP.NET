using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ZUVO_MVC_.ViewModels
{
    public class CarViewModel
    {
        public string? CarId { get; set; }

        [Required(ErrorMessage = "Host ID is required")]
        public string HostId { get; set; }

        [Required(ErrorMessage = "Make is required")]
        [StringLength(100, ErrorMessage = "Make cannot exceed 100 characters")]
        public string Make { get; set; }

        [Required(ErrorMessage = "Model is required")]
        [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Year is required")]
        [Range(1900, 2100, ErrorMessage = "Please enter a valid year")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Color is required")]
        [StringLength(50, ErrorMessage = "Color cannot exceed 50 characters")]
        public string Color { get; set; }

        [Required(ErrorMessage = "License plate number is required")]
        [StringLength(20, ErrorMessage = "License plate number cannot exceed 20 characters")]
        public string LicensePlateNo { get; set; }

        [Required(ErrorMessage = "VIN is required")]
        [StringLength(17, ErrorMessage = "VIN must be exactly 17 characters")]
        public string VIN { get; set; }

        [Required(ErrorMessage = "Transmission type is required")]
        [StringLength(50, ErrorMessage = "Transmission type cannot exceed 50 characters")]
        public string Transmission { get; set; }

        [Required(ErrorMessage = "Fuel type is required")]
        [StringLength(50, ErrorMessage = "Fuel type cannot exceed 50 characters")]
        public string FuelType { get; set; }

        [Required(ErrorMessage = "Number of seats is required")]
        [Range(1, 10, ErrorMessage = "Number of seats must be between 1 and 10")]
        public int Seats { get; set; }

        [Required(ErrorMessage = "Daily rate is required")]
        [Range(0.01, 10000.00, ErrorMessage = "Daily rate must be between 0.01 and 10000.00")]
        public decimal DailyRate { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Cancellation policy is required")]
        public bool AllowCancellation { get; set; } = true;

        [Required(ErrorMessage = "Minimum rental period is required")]
        [Range(1, 365, ErrorMessage = "Minimum rental period must be between 1 and 365 days")]
        public int MinRentalPeriod { get; set; } = 1;

        [Required(ErrorMessage = "Mileage is required")]
        [StringLength(50, ErrorMessage = "Mileage cannot exceed 50 characters")]
        public string Mileage { get; set; }

        [Required(ErrorMessage = "Additional features are required")]
        [StringLength(100, ErrorMessage = "Additional features cannot exceed 100 characters")]
        public string AdditionalFeatures { get; set; }

        [Required(ErrorMessage = "Insurance type is required")]
        [StringLength(50, ErrorMessage = "Insurance type cannot exceed 50 characters")]
        public string InsuranceType { get; set; }

        [Required(ErrorMessage = "Insurance number is required")]
        [StringLength(50, ErrorMessage = "Insurance number cannot exceed 50 characters")]
        public string InsuranceNumber { get; set; }

        [Required(ErrorMessage = "Insurance company is required")]
        [StringLength(100, ErrorMessage = "Insurance company cannot exceed 100 characters")]
        public string InsuranceCompany { get; set; }

        [Required(ErrorMessage = "Car registration document is required")]
        public IFormFile CarRegistrationDocument { get; set; }

        [Required(ErrorMessage = "Insurance certificate is required")]
        public IFormFile InsuranceCertificate { get; set; }

        [Required(ErrorMessage = "At least one photo is required")]
        public List<IFormFile> CarPhotos { get; set; }
        public List<string> UploadedPhotoPaths { get; set; } = new List<string>();

        public bool IsAvailable { get; set; } = true;
    }
} 