using System.ComponentModel.DataAnnotations;

namespace AuctionX.ViewModels
{
    public class RegisterPlayerViewModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.[!@#$%^&(),.?""':{}|<>])[A-Za-z\d!@#$%^&*(),.?""':{}|<>]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and contain at least one special character.")]
        public string Password { get; set; }

        [Required]
        [RegularExpression(@"^[6-9]\d{9}$",
            ErrorMessage = "Enter a valid 10-digit mobile number starting with 6, 7, 8, or 9.")]
        public string MobileNumber { get; set; }

        public string? Description { get; set; }
    }
}