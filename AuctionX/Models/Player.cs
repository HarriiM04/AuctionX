using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuctionX.Models;

public partial class Player
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public string? Photo { get; set; }

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^(?!1234567890$)([6-9]{1}[0-9]{9})$", ErrorMessage = "Enter a valid 10-digit Indian mobile number.")]
    public string MobileNumber { get; set; } = null!;
    [Required]
    [MaxLength(500, ErrorMessage = "Description can't be longer than 500 characters.")]
    public string? Description { get; set; }
    [Required(ErrorMessage = "Availability status is required.")]
    [RegularExpression(@"^(Available|Unavailable)$", ErrorMessage = "Availability status must be either 'Available' or 'Unavailable'.")]
    public string AvailabilityStatus { get; set; } = null!;

    public virtual ICollection<Bidder> Bidders { get; set; } = new List<Bidder>();

    public virtual ICollection<Bid> Bids { get; set; } = new List<Bid>();

    public virtual ICollection<PlayerSport> PlayerSports { get; set; } = new List<PlayerSport>();

    public virtual ICollection<TeamPlayer> TeamPlayers { get; set; } = new List<TeamPlayer>();

    public virtual ICollection<TournamentPlayer> TournamentPlayers { get; set; } = new List<TournamentPlayer>();

    public virtual ICollection<TournamentResult> TournamentResults { get; set; } = new List<TournamentResult>();

    public virtual User? User { get; set; }
}
