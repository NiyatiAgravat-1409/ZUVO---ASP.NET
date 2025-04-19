using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZUVO_MVC_.Models
{
    public class CarType
    {
        [Key]
        public string CarTypeId { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerDay { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        public int BookCount { get; set; }
        public int NumberOfSeats { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
