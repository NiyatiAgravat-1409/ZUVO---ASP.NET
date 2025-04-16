    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZUVO_MVC_.Models
{
    public class Car
    {
        [Key]
        public string CarId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string HostId { get; set; }

        [Required]
        [StringLength(100)]
        public string Make { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        [StringLength(50)]
        public string Color { get; set; }

        [Required]
        [StringLength(20)]
        public string LicensePlateNo { get; set; }

        [Required]
        [StringLength(17)]
        public string VIN { get; set; }

        [Required]
        [StringLength(50)]
        public string Transmission { get; set; }

        [Required]
        [StringLength(50)]
        public string FuelType { get; set; }

        [Required]
        public int Seats { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyRate { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public bool AllowCancellation { get; set; } = true;

        [Required]
        public int MinRentalPeriod { get; set; } = 1; // in days

        [Required]
        [StringLength(50)]
        public string Mileage { get; set; }

        [Required]
        [StringLength(100)]
        public string AdditionalFeatures { get; set; }

        [Required]
        [StringLength(50)]
        public string InsuranceType { get; set; }

        [Required]
        [StringLength(50)]
        public string InsuranceNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string InsuranceCompany { get; set; }

        [Required]
        public string CarRegistrationDocumentPath { get; set; }

        [Required]
        public string InsuranceCertificatePath { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual HostUser Host { get; set; }
        public virtual ICollection<CarPhoto> Photos { get; set; }
    }
} 