using System.ComponentModel.DataAnnotations;

namespace AuctionX.ViewModels
{
    public class CreateOrganizerViewModel
    {
        [Required]
        [RegularExpression(@"^[A-Za-z\s]{3,50}$", ErrorMessage = "Name must be 3–50 characters and contain only letters and spaces.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Enter a valid email like example@mail.com.")]
        public string Email { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*]).{6,}$",
    ErrorMessage = "Password must be at least 6 characters and contain a number, uppercase letter, and special character.")]

        public string Password { get; set; } = null!;

        [Required]
        [RegularExpression(@"^(?!1234567890$)([6-9]{1}[0-9]{9})$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
        public string MobileNumber { get; set; } = null!;
    }

}
