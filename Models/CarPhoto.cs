using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZUVO_MVC_.Models
{
    public class CarPhoto
    {
        [Key]
        public string PhotoId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string CarId { get; set; }

        [Required]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; }

        [Required]
        public bool IsPrimary { get; set; } = false;

        [Required]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual Car Car { get; set; }
    }
} 