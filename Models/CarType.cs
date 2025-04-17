using System.ComponentModel.DataAnnotations;

namespace ZUVO.Models
{
    public class CarType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Car Type")]
        public string CarTypeName { get; set; }

        [Required]
        [Display(Name = "Car Name")]
        public string CarName { get; set; }

        [Required]
        [Range(2, 10)]
        [Display(Name = "Number of Seats")]
        public int NumberOfSeats { get; set; }

        [Required]
        [Display(Name = "Fuel Type")]
        public string Fuel { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Price Per Day")]
        public decimal PricePerDay { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        [Display(Name = "Booking Count")]
        public int BookCount { get; set; }
    }
}
