using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuctionX.Models;

public partial class Organizer
{
    public int Id { get; set; }

    public int? UserId { get; set; }


    [Required]
    [RegularExpression(@"^(?!1234567890$)([6-9]{1}[0-9]{9})$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
    public string MobileNumber { get; set; } = null!;

    public virtual ICollection<Tournament> Tournaments { get; set; } = new List<Tournament>();

    public virtual User? User { get; set; }
}
