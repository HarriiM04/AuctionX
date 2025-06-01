using System.ComponentModel.DataAnnotations;

namespace AuctionX.ViewModels
{
    public class PlayerEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [RegularExpression(@"^[A-Za-z\s]{3,50}$", ErrorMessage = "Name must be 3–50 characters and contain only letters and spaces.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Enter a valid email like example@mail.com.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^(?!1234567890$)([6-9]{1}[0-9]{9})$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
        public string MobileNumber { get; set; }

        [Required]
        [MaxLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Availability status is required.")]
        [RegularExpression(@"^(Available|Unavailable)$", ErrorMessage = "Availability status must be either 'Available' or 'Unavailable'.")]
        public string AvailabilityStatus { get; set; }
    }
}
