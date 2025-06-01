using System.ComponentModel.DataAnnotations;

namespace AuctionX.Models
{
    public class PlayerPasswordViewModel
    {
        public int PlayerId { get; set; }
        public string OriginalPassword { get; set; }
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "New password and confirm password do not match.")]
        public string ConfirmPassword { get; set; }  // Correct name
    }
}
