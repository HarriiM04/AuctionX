using System;
using System.Collections.Generic;

namespace AuctionX.Models;

public partial class TournamentPlayer
{
    public int Id { get; set; }

    public int? PlayerId { get; set; }

    public int? TournamentId { get; set; }

    public string AvailabilityStatus { get; set; } = null!;

    public virtual Player? Player { get; set; }

    public virtual Tournament? Tournament { get; set; }
}
