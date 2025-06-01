using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class TeamPlayer
{
    public int Id { get; set; }

    public int? TeamId { get; set; }

    public int? PlayerId { get; set; }

    public int? TournamentId { get; set; }

    public virtual Player? Player { get; set; }

    public virtual Team? Team { get; set; }

    public virtual Tournament? Tournament { get; set; }
}
