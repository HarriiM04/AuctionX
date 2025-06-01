using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class Team
{
    public int Id { get; set; }

    public int? TournamentId { get; set; }

    public string TeamName { get; set; } = null!;

    public decimal Budget { get; set; }

    public int? CaptainId { get; set; }

    public virtual ICollection<Bidder> Bidders { get; set; } = new List<Bidder>();

    public virtual Bidder? Captain { get; set; }

    public virtual ICollection<TeamPlayer> TeamPlayers { get; set; } = new List<TeamPlayer>();

    public virtual Tournament? Tournament { get; set; }

    public virtual ICollection<TournamentResult> TournamentResults { get; set; } = new List<TournamentResult>();
}
