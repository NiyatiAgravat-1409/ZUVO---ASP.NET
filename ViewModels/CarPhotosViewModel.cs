using System.ComponentModel.DataAnnotations;

namespace ZUVO_MVC_.ViewModels
{
    public class CarPhotosViewModel
    {
        [Required]
        public string CarId { get; set; }

        [Required]
        public List<IFormFile> CarPhotos { get; set; }
    }

}
