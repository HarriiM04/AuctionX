using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AuctionX.Models;

public partial class Tournament
{
    public int Id { get; set; }

    public string? Logo { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = null!;

    public DateTime Date { get; set; }

    [Required(ErrorMessage = "Venue is required")]
    [StringLength(100, ErrorMessage = "Venue detail cannot exceed 150 characters")]
    public string Venue { get; set; } = null!;

    public int? OrganizerId { get; set; }

    public int? SportId { get; set; }

    public virtual ICollection<BidMaster> BidMasters { get; set; } = new List<BidMaster>();

    public virtual ICollection<Bidder> Bidders { get; set; } = new List<Bidder>();

    public virtual Organizer? Organizer { get; set; }

    public virtual Sport? Sport { get; set; }

    public virtual ICollection<TeamPlayer> TeamPlayers { get; set; } = new List<TeamPlayer>();

    public virtual ICollection<Team> Teams { get; set; } = new List<Team>();

    public virtual ICollection<TournamentPlayer> TournamentPlayers { get; set; } = new List<TournamentPlayer>();

    public virtual ICollection<TournamentResult> TournamentResults { get; set; } = new List<TournamentResult>();
}
