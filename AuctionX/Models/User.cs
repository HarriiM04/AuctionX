using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuctionX.Models;

public partial class User
{
    public int Id { get; set; }

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

    public string Role { get; set; } = null!;

    public virtual ICollection<Admin> Admins { get; set; } = new List<Admin>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Organizer> Organizers { get; set; } = new List<Organizer>();

    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
}
